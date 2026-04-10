using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BlueMeter.Services.ModuleSolver.Models;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace BlueMeter.Services.ModuleSolver;

/// <summary>
/// Service for capturing module data from game screen using OCR.
///
/// Direct port of the WPF version. Zero framework dependencies — uses
/// <see cref="System.Drawing"/> for screen capture (already referenced from
/// <c>QueuePopUIDetector</c>), Win32 P/Invoke (<c>user32</c>/<c>gdi32</c>) for
/// <c>PrintWindow</c>-based window capture, and <see cref="TesseractEngine"/>
/// for OCR. No WPF types touched.
/// </summary>
public class ModuleOCRCaptureService : IDisposable
{
    private readonly ILogger<ModuleOCRCaptureService> _logger;
    private TesseractEngine? _ocrEngine;
    private readonly SemaphoreSlim _ocrLock = new(1, 1);
    private bool _disposed;
    private StreamWriter? _debugLog;

    // Game process names to monitor
    private static readonly string[] GameProcessNames =
    {
        "star",           // Original game
        "BPSR_STEAM",     // Steam version
        "BPSR_EPIC",      // Epic version
        "BPSR"            // Generic version
    };

    // P/Invoke declarations for screen capture
    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    private const uint PW_RENDERFULLCONTENT = 0x00000002;

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public event EventHandler<List<ModuleInfo>>? ModuleCaptured;

    private List<ModuleInfo> _capturedModules = new();

    public ModuleOCRCaptureService(ILogger<ModuleOCRCaptureService> logger)
    {
        _logger = logger;

        // Create debug log file
        try
        {
            var debugFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ocr_debug");
            if (!Directory.Exists(debugFolder))
                Directory.CreateDirectory(debugFolder);

            var debugLogPath = Path.Combine(debugFolder, $"ocr_debug_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            _debugLog = new StreamWriter(debugLogPath, true) { AutoFlush = true };
            WriteDebug("=== OCR Debug Log Started ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create debug log");
        }

        InitializeOCR();
    }

    private void WriteDebug(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _debugLog?.WriteLine($"[{timestamp}] {message}");
            Console.WriteLine(message);
        }
        catch
        {
            // Ignore write errors
        }
    }

    private void InitializeOCR()
    {
        try
        {
            var tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            if (!Directory.Exists(tessDataPath))
            {
                _logger.LogError("[Module OCR] tessdata directory not found at: {Path}", tessDataPath);
                return;
            }

            _ocrEngine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            _logger.LogInformation("[Module OCR] Tesseract engine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Module OCR] Failed to initialize Tesseract engine");
        }
    }

    /// <summary>
    /// Captures a single module from the current game window
    /// Call this method repeatedly as the player navigates through modules
    /// </summary>
    public ModuleInfo? CaptureCurrentModule()
    {
        _logger.LogDebug("[Module OCR] CaptureCurrentModule called");

        if (_ocrEngine == null)
        {
            _logger.LogError("[Module OCR] OCR engine not initialized!");
            return null;
        }

        try
        {
            var gameProcess = FindGameProcess();
            if (gameProcess == null)
            {
                _logger.LogDebug("[Module OCR] Game process not found. Looking for: {Processes}",
                    string.Join(", ", GameProcessNames));
                return null;
            }

            _logger.LogInformation("[Module OCR] Found game process: {Name} (PID: {Pid})",
                gameProcess.ProcessName, gameProcess.Id);

            var mainWindowHandle = gameProcess.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                _logger.LogWarning("[Module OCR] Game window handle is zero - game might be minimized");
                return null;
            }

            _logger.LogInformation("[Module OCR] Capturing window with handle: {Handle}", mainWindowHandle);

            using var screenshot = CaptureWindow(mainWindowHandle);
            if (screenshot == null)
            {
                _logger.LogError("[Module OCR] Failed to capture window screenshot!");
                return null;
            }

            _logger.LogInformation("[Module OCR] Screenshot captured: {Width}x{Height}",
                screenshot.Width, screenshot.Height);

            var moduleInfo = ExtractModuleInfo(screenshot);

            if (moduleInfo != null)
            {
                // Check if module already exists (by UUID or by matching properties)
                var exists = _capturedModules.Any(m =>
                    m.Uuid == moduleInfo.Uuid ||
                    (m.Name == moduleInfo.Name && m.Quality == moduleInfo.Quality &&
                     m.Parts.Count == moduleInfo.Parts.Count &&
                     m.Parts.All(p1 => moduleInfo.Parts.Any(p2 => p1.Name == p2.Name && p1.Value == p2.Value))));

                if (!exists)
                {
                    _capturedModules.Add(moduleInfo);
                    _logger.LogInformation("[Module OCR] Captured module #{Count}: {Name} Q{Quality}",
                        _capturedModules.Count, moduleInfo.Name, moduleInfo.Quality);
                    WriteDebug($"[Module OCR] ADDED TO LIST: #{_capturedModules.Count} - {moduleInfo.Name}");
                }
                else
                {
                    _logger.LogDebug("[Module OCR] Duplicate module detected, skipping");
                    WriteDebug($"[Module OCR] DUPLICATE SKIPPED: {moduleInfo.Name}");
                }
            }
            else
            {
                WriteDebug("[Module OCR] Module parsing returned NULL");
            }

            return moduleInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Module OCR] Error capturing module");
            return null;
        }
    }

    /// <summary>
    /// Gets all captured modules
    /// </summary>
    public List<ModuleInfo> GetCapturedModules()
    {
        return new List<ModuleInfo>(_capturedModules);
    }

    /// <summary>
    /// Clears all captured modules
    /// </summary>
    public void ClearCapturedModules()
    {
        _capturedModules.Clear();
        _logger.LogInformation("[Module OCR] Cleared all captured modules");
    }

    /// <summary>
    /// Finalizes capture and raises event with all captured modules
    /// </summary>
    public void FinalizeCaptureSession()
    {
        WriteDebug($"[Module OCR] === FINALIZE CAPTURE SESSION ===");
        WriteDebug($"[Module OCR] Total captured modules in list: {_capturedModules.Count}");

        if (_capturedModules.Count > 0)
        {
            _logger.LogInformation("[Module OCR] Capture session finalized with {Count} modules", _capturedModules.Count);
            WriteDebug($"[Module OCR] Raising ModuleCaptured event with {_capturedModules.Count} modules");

            foreach (var mod in _capturedModules)
            {
                WriteDebug($"[Module OCR]   - {mod.Name} ({mod.Parts.Count} parts)");
            }

            ModuleCaptured?.Invoke(this, new List<ModuleInfo>(_capturedModules));
            WriteDebug($"[Module OCR] Event raised successfully");
        }
        else
        {
            WriteDebug($"[Module OCR] NO MODULES CAPTURED!");
        }
    }

    private Process? FindGameProcess()
    {
        foreach (var processName in GameProcessNames)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
                return processes[0];
        }

        return null;
    }

    private Bitmap? CaptureWindow(IntPtr hWnd)
    {
        IntPtr hdcSrc = IntPtr.Zero;
        IntPtr hdcDest = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;

        try
        {
            var rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
                return null;

            hdcSrc = GetDC(hWnd);
            if (hdcSrc == IntPtr.Zero)
                return null;

            hdcDest = CreateCompatibleDC(hdcSrc);
            if (hdcDest == IntPtr.Zero)
                return null;

            hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            if (hBitmap == IntPtr.Zero)
                return null;

            var hOld = SelectObject(hdcDest, hBitmap);
            bool success = PrintWindow(hWnd, hdcDest, PW_RENDERFULLCONTENT);
            SelectObject(hdcDest, hOld);

            if (!success)
                return null;

            return Image.FromHbitmap(hBitmap);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Module OCR] Error capturing window");
            return null;
        }
        finally
        {
            if (hBitmap != IntPtr.Zero)
                DeleteObject(hBitmap);
            if (hdcDest != IntPtr.Zero)
                DeleteDC(hdcDest);
            if (hdcSrc != IntPtr.Zero)
                ReleaseDC(hWnd, hdcSrc);
        }
    }

    private ModuleInfo? ExtractModuleInfo(Bitmap screenshot)
    {
        if (!_ocrLock.Wait(5000))
        {
            _logger.LogWarning("[Module OCR] Could not acquire OCR lock");
            return null;
        }

        try
        {
            if (_ocrEngine == null || _disposed)
            {
                _logger.LogWarning("[Module OCR] OCR engine is null or disposed");
                return null;
            }

            // Save screenshot for debugging (optional)
            var debugFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ocr_debug");
            if (!Directory.Exists(debugFolder))
                Directory.CreateDirectory(debugFolder);

            var debugPath = Path.Combine(debugFolder, $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            screenshot.Save(debugPath, System.Drawing.Imaging.ImageFormat.Png);
            _logger.LogInformation("[Module OCR] Saved debug screenshot to: {Path}", debugPath);

            // Save debug info to console (always visible)
            WriteDebug($"[Module OCR] Screenshot saved: {Path.GetFileName(debugPath)}");

            using var ms = new MemoryStream();
            screenshot.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var img = Pix.LoadFromMemory(ms.ToArray());
            using var page = _ocrEngine.Process(img);

            var text = page.GetText() ?? string.Empty;

            _logger.LogInformation("[Module OCR] OCR extracted {Length} characters of text", text.Length);
            _logger.LogInformation("[Module OCR] Full OCR text:\n{Text}", text);

            // Save OCR text to file for debugging
            var textPath = Path.Combine(debugFolder, $"ocr_text_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(textPath, text);
            WriteDebug($"[Module OCR] OCR text saved: {Path.GetFileName(textPath)} ({text.Length} chars)");
            WriteDebug($"[Module OCR] First 200 chars: {(text.Length > 200 ? text.Substring(0, 200) : text)}");

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("[Module OCR] OCR returned empty text!");
                WriteDebug("[Module OCR] WARNING: OCR returned empty text!");
                return null;
            }

            // Parse module info from OCR text
            return ParseModuleFromText(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Module OCR] Error extracting module info");
            return null;
        }
        finally
        {
            _ocrLock.Release();
        }
    }

    private ModuleInfo? ParseModuleFromText(string text)
    {
        try
        {
            // Normalize text - replace multiple spaces/newlines with single space
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.Multiline);

            // Look for module title patterns like:
            // "Excellent Attack Module"
            // "Superior Defense Module"
            // etc.

            // Find module type (Attack/Defense/Support) - more flexible pattern
            // Allow for variations like "Attack", "Offensive", "Guard", "Defensive", "Healing", "Support"
            var typeMatch = Regex.Match(text,
                @"(Attack|Offensive?|Defen[cs]e|Defensive?|Guard|Protection|Support|Healing?)\s*(?:Module)?",
                RegexOptions.IgnoreCase);

            // If no match with flexible pattern, try to find just the word "Module" and look nearby
            if (!typeMatch.Success)
            {
                var moduleMatch = Regex.Match(text, @"Module", RegexOptions.IgnoreCase);
                if (moduleMatch.Success)
                {
                    // Look for category words near "Module" (within 20 characters)
                    var nearbyText = text.Substring(Math.Max(0, moduleMatch.Index - 20),
                        Math.Min(40, text.Length - Math.Max(0, moduleMatch.Index - 20)));
                    typeMatch = Regex.Match(nearbyText,
                        @"(Attack|Offensive?|Defen[cs]e|Defensive?|Guard|Protection|Support|Healing?)",
                        RegexOptions.IgnoreCase);
                }
            }

            if (!typeMatch.Success)
            {
                _logger.LogDebug("[Module OCR] Could not find module type in text");
                WriteDebug("[Module OCR] Could not find module type in text");
                WriteDebug($"[Module OCR] Text preview: {text.Substring(0, Math.Min(200, text.Length))}");
                return null;
            }

            WriteDebug($"[Module OCR] Found module type: {typeMatch.Value}");

            var categoryStr = typeMatch.Groups[1].Value;
            var category = categoryStr.ToLower() switch
            {
                "attack" or "offense" or "offensive" => ModuleCategory.Attack,
                "defense" or "defence" or "defensive" or "guard" or "protection" => ModuleCategory.Defense,
                "support" or "healing" or "heal" => ModuleCategory.Support,
                _ => ModuleCategory.All
            };

            // Find quality/rarity - look for quality keywords anywhere in text
            var qualityMatch = Regex.Match(text,
                @"(Excellent|Superior|Advanced|Rare|Epic|Legendary|Common|Uncommon|Basic|High\s*Performance)",
                RegexOptions.IgnoreCase);
            var qualityStr = qualityMatch.Success ? qualityMatch.Groups[1].Value : "Common";

            var quality = qualityStr.ToLower() switch
            {
                "legendary" => 6,
                "epic" or "excellent" => 5,
                "superior" or "rare" or "advanced" => 4,
                "uncommon" or "high performance" => 3,
                "common" or "basic" => 2,
                _ => 3
            };

            // Find all attribute patterns like:
            // "Agile+1", "Special Attack+9", "Agility Boost+4"
            // "ATK+12", "HP Boost+5", "CRIT DMG+8"
            // Also handle variations like "Agile + 1", "Special Attack: 9", "Agility Boost 4"
            var parts = new List<ModulePart>();

            // More flexible pattern for attributes with numbers
            // Matches various formats: "Name+9", "Name + 9", "Name: 9", "Name 9"
            // Also handles OCR variations like "|" instead of "I" in numbers
            var attributeMatches = Regex.Matches(text,
                @"([A-Za-z][\w\s&]*?)\s*(?:\+|:|\s)\s*([0-9|Il]+)",
                RegexOptions.IgnoreCase);

            WriteDebug($"[Module OCR] Found {attributeMatches.Count} potential attribute matches");

            int partId = 0;
            var processedAttributes = new HashSet<string>();

            foreach (Match match in attributeMatches)
            {
                var ocrText = match.Groups[1].Value.Trim();
                var valueStr = match.Groups[2].Value;

                // Skip if this looks like a non-attribute match
                if (ocrText.Length < 2 || ocrText.Contains("Link") || ocrText.Contains("Module"))
                {
                    WriteDebug($"[Module OCR]   Skipped: '{ocrText}' (too short or contains Link/Module)");
                    continue;
                }

                // Map OCR text to game attribute ID and name
                var (attrId, attrName) = NormalizeAttributeToGameFormat(ocrText);

                // Skip unknown attributes or duplicates
                if (attrId == 0)
                {
                    WriteDebug($"[Module OCR]   Unknown: '{ocrText}' -> no mapping found");
                    continue;
                }
                if (processedAttributes.Contains(attrName))
                {
                    WriteDebug($"[Module OCR]   Duplicate: '{attrName}'");
                    continue;
                }

                // Clean up OCR errors in numbers (| -> 1, l -> 1, I -> 1, O -> 0, etc.)
                valueStr = valueStr.Replace("|", "1").Replace("l", "1").Replace("I", "1").Replace("O", "0");

                if (int.TryParse(valueStr, out var value) && value > 0)
                {
                    parts.Add(new ModulePart(attrId, attrName, value));
                    processedAttributes.Add(attrName);
                    _logger.LogDebug("[Module OCR] Found attribute: {Name}+{Value} (ID: {Id})", attrName, value, attrId);
                    WriteDebug($"[Module OCR]   Parsed: '{ocrText}' -> {attrName}+{value}");
                }
                else
                {
                    WriteDebug($"[Module OCR]   Invalid value: '{valueStr}' for attribute '{ocrText}'");
                }
            }

            if (parts.Count == 0)
            {
                _logger.LogDebug("[Module OCR] No attributes found in text");
                WriteDebug("[Module OCR] No valid attributes parsed!");
                return null;
            }

            WriteDebug($"[Module OCR] Successfully parsed {parts.Count} attributes");

            var moduleName = qualityMatch.Success
                ? $"{qualityStr} {categoryStr} Module"
                : $"{categoryStr} Module";

            var module = new ModuleInfo
            {
                Name = moduleName,
                ConfigId = quality * 100 + (int)category, // Unique config ID based on quality and category
                Uuid = DateTime.Now.Ticks + partId, // Unique UUID with slight variation
                Quality = quality,
                Parts = parts,
                Category = category
            };

            _logger.LogInformation("[Module OCR] Parsed module: {Name} with {Count} attributes",
                moduleName, parts.Count);

            WriteDebug($"[Module OCR] PARSED: {moduleName} with {parts.Count} attributes");
            foreach (var part in parts)
            {
                WriteDebug($"           - {part.Name}+{part.Value}");
            }

            return module;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Module OCR] Error parsing module text");
            return null;
        }
    }

    private (int id, string name) NormalizeAttributeToGameFormat(string ocrText)
    {
        // Clean up the OCR text - normalize spaces and remove special characters
        ocrText = Regex.Replace(ocrText.Trim(), @"\s+", " ");
        ocrText = ocrText.Replace("&", "and");

        // Map OCR text to ModuleConstants attribute IDs and names
        // This ensures compatibility with the optimizer algorithm
        var attributeMappings = new Dictionary<string, (int id, string name)>(StringComparer.OrdinalIgnoreCase)
        {
            // From ModuleConstants.AttributeNames - with additional OCR variations
            { "Strength Boost", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },
            { "Strength", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },
            { "STR Boost", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },
            { "STR", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },
            { "Str Boost", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },
            { "Str", (ModuleConstants.STRENGTH_BOOST, "Strength Boost") },

            { "Agility Boost", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },
            { "AGI Boost", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },
            { "Agility", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },
            { "AGI", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },
            { "Agi Boost", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },
            { "Agi", (ModuleConstants.AGILITY_BOOST, "Agility Boost") },

            { "Intellect Boost", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Intelligence Boost", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "INT Boost", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "INT", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Intel Boost", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Int Boost", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Int", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Intelligence", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },
            { "Intellect", (ModuleConstants.INTELLIGENCE_BOOST, "Intellect Boost") },

            { "Special Attack", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "Special ATK", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "SpecialAttack", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "Special Atk", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "Spec Attack", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "Spec Atk", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },
            { "Special", (ModuleConstants.SPECIAL_ATTACK_DAMAGE, "Special Attack") },

            { "Elite Strike", (ModuleConstants.ELITE_STRIKE, "Elite Strike") },
            { "Elite", (ModuleConstants.ELITE_STRIKE, "Elite Strike") },

            { "Healing Boost", (ModuleConstants.SPECIAL_HEALING_BOOST, "Healing Boost") },
            { "Heal Boost", (ModuleConstants.SPECIAL_HEALING_BOOST, "Healing Boost") },
            { "Healing", (ModuleConstants.SPECIAL_HEALING_BOOST, "Healing Boost") },
            { "Heal", (ModuleConstants.SPECIAL_HEALING_BOOST, "Healing Boost") },

            { "Healing Enhance", (ModuleConstants.EXPERT_HEALING_BOOST, "Healing Enhance") },
            { "Heal Enhance", (ModuleConstants.EXPERT_HEALING_BOOST, "Healing Enhance") },
            { "Healing Enhancement", (ModuleConstants.EXPERT_HEALING_BOOST, "Healing Enhance") },
            { "Enhance", (ModuleConstants.EXPERT_HEALING_BOOST, "Healing Enhance") },

            { "Cast Focus", (ModuleConstants.CASTING_FOCUS, "Cast Focus") },
            { "Casting Focus", (ModuleConstants.CASTING_FOCUS, "Cast Focus") },
            { "Cast", (ModuleConstants.CASTING_FOCUS, "Cast Focus") },
            { "Casting", (ModuleConstants.CASTING_FOCUS, "Cast Focus") },

            { "Attack SPD", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "Attack Speed", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "ATK SPD", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "Speed", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "Atk Spd", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "Attack Spd", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },
            { "ATK Speed", (ModuleConstants.ATTACK_SPEED_FOCUS, "Attack SPD") },

            { "Crit Focus", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },
            { "Critical Focus", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },
            { "CRIT Focus", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },
            { "CRIT", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },
            { "Critical", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },
            { "Crit", (ModuleConstants.CRITICAL_FOCUS, "Crit Focus") },

            { "Luck Focus", (ModuleConstants.LUCK_FOCUS, "Luck Focus") },
            { "Luck", (ModuleConstants.LUCK_FOCUS, "Luck Focus") },

            { "Resistance", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },
            { "Magic Resistance", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },
            { "RES", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },
            { "Res", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },
            { "Magic Res", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },
            { "Mag Res", (ModuleConstants.MAGIC_RESISTANCE, "Resistance") },

            { "Armor", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "Physical Resistance", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "DEF", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "Def", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "Defense", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "Physical Res", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },
            { "Phys Res", (ModuleConstants.PHYSICAL_RESISTANCE, "Armor") },

            { "DMG Stack", (ModuleConstants.EXTREME_DAMAGE_STACK, "DMG Stack") },
            { "Damage Stack", (ModuleConstants.EXTREME_DAMAGE_STACK, "DMG Stack") },
            { "Dmg Stack", (ModuleConstants.EXTREME_DAMAGE_STACK, "DMG Stack") },
            { "Damage", (ModuleConstants.EXTREME_DAMAGE_STACK, "DMG Stack") },

            { "Agile", (ModuleConstants.EXTREME_FLEXIBLE_MOVEMENT, "Agile") },
            { "Flexible Movement", (ModuleConstants.EXTREME_FLEXIBLE_MOVEMENT, "Agile") },
            { "Flexible", (ModuleConstants.EXTREME_FLEXIBLE_MOVEMENT, "Agile") },
            { "Movement", (ModuleConstants.EXTREME_FLEXIBLE_MOVEMENT, "Agile") },

            { "Life Condense", (ModuleConstants.EXTREME_LIFE_CONVERGENCE, "Life Condense") },
            { "Life Convergence", (ModuleConstants.EXTREME_LIFE_CONVERGENCE, "Life Condense") },
            { "Life Cond", (ModuleConstants.EXTREME_LIFE_CONVERGENCE, "Life Condense") },

            { "First Aid", (ModuleConstants.EXTREME_EMERGENCY_MEASURES, "First Aid") },
            { "Emergency Measures", (ModuleConstants.EXTREME_EMERGENCY_MEASURES, "First Aid") },
            { "Emergency", (ModuleConstants.EXTREME_EMERGENCY_MEASURES, "First Aid") },
            { "Aid", (ModuleConstants.EXTREME_EMERGENCY_MEASURES, "First Aid") },

            { "Life Wave", (ModuleConstants.EXTREME_LIFE_FLUCTUATION, "Life Wave") },
            { "Life Fluctuation", (ModuleConstants.EXTREME_LIFE_FLUCTUATION, "Life Wave") },
            { "Life Fluct", (ModuleConstants.EXTREME_LIFE_FLUCTUATION, "Life Wave") },

            { "Life Steal", (ModuleConstants.EXTREME_LIFE_DRAIN, "Life Steal") },
            { "Life Drain", (ModuleConstants.EXTREME_LIFE_DRAIN, "Life Steal") },
            { "Lifesteal", (ModuleConstants.EXTREME_LIFE_DRAIN, "Life Steal") },

            { "Team Luck&Crit", (ModuleConstants.EXTREME_TEAM_CRIT, "Team Luck&Crit") },
            { "Team Crit", (ModuleConstants.EXTREME_TEAM_CRIT, "Team Luck&Crit") },
            { "Team Luck and Crit", (ModuleConstants.EXTREME_TEAM_CRIT, "Team Luck&Crit") },
            { "Team Luck Crit", (ModuleConstants.EXTREME_TEAM_CRIT, "Team Luck&Crit") },

            { "Final Protection", (ModuleConstants.EXTREME_DESPERATE_GUARDIAN, "Final Protection") },
            { "Desperate Guardian", (ModuleConstants.EXTREME_DESPERATE_GUARDIAN, "Final Protection") },
            { "Final Prot", (ModuleConstants.EXTREME_DESPERATE_GUARDIAN, "Final Protection") },
            { "Desperate", (ModuleConstants.EXTREME_DESPERATE_GUARDIAN, "Final Protection") }
        };

        // Try to find exact match
        if (attributeMappings.TryGetValue(ocrText, out var result))
        {
            return result;
        }

        // Try partial match (for OCR errors)
        // First try to find the best match based on similarity
        var bestMatch = (key: "", value: (id: 0, name: ""), score: 0.0);

        foreach (var mapping in attributeMappings)
        {
            // Check if either contains the other
            if (ocrText.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
            {
                var score = (double)mapping.Key.Length / ocrText.Length;
                if (score > bestMatch.score)
                {
                    bestMatch = (mapping.Key, mapping.Value, score);
                }
            }
            else if (mapping.Key.Contains(ocrText, StringComparison.OrdinalIgnoreCase))
            {
                var score = (double)ocrText.Length / mapping.Key.Length;
                if (score > bestMatch.score)
                {
                    bestMatch = (mapping.Key, mapping.Value, score);
                }
            }
            // Check if they share significant common words
            else
            {
                var ocrWords = ocrText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var mapWords = mapping.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var commonWords = ocrWords.Intersect(mapWords, StringComparer.OrdinalIgnoreCase).Count();
                if (commonWords > 0)
                {
                    var score = (double)commonWords / Math.Max(ocrWords.Length, mapWords.Length);
                    if (score > 0.5 && score > bestMatch.score)
                    {
                        bestMatch = (mapping.Key, mapping.Value, score);
                    }
                }
            }
        }

        if (bestMatch.score > 0.5)
        {
            WriteDebug($"[Module OCR]   Fuzzy match: '{ocrText}' -> {bestMatch.value.name} (score: {bestMatch.score:F2})");
            return bestMatch.value;
        }

        // Default: return with unknown ID
        WriteDebug($"[Module OCR]   No match for: '{ocrText}'");
        return (0, ocrText);
    }

    private int ConvertRomanToInt(string roman)
    {
        return roman.ToUpper() switch
        {
            "I" => 1,
            "II" => 2,
            "III" => 3,
            "IV" => 4,
            "V" => 5,
            "VI" => 6,
            _ => 1
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _ocrLock.Wait(TimeSpan.FromSeconds(2));
            _ocrLock.Release();
        }
        catch
        {
            // Ignore timeout
        }

        try
        {
            _ocrEngine?.Dispose();
            _ocrEngine = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Module OCR] Error disposing OCR engine");
        }

        try
        {
            _ocrLock.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}
