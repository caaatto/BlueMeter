# Queue Pop Alert System - Implementation Plan

## Overview

Implement an audio alert system that plays a sound when a dungeon/raid queue pops (when players connect for matchmaking).

## Available Sounds

Located in `BlueMeter.Assets/sounds/`:
- `drum.mp3` (244 KB)
- `harp.mp3` (128 KB)
- `wow.mp3` (98 KB)
- `yoooo.mp3` (265 KB)

---

## Phase 1: Sound Infrastructure & Settings UI ✅ (START HERE)

### 1.1 Sound Player Service

**File**: `BlueMeter.WPF/Services/SoundPlayerService.cs`

```csharp
public interface ISoundPlayerService
{
    void PlayQueuePopSound();
    void TestSound(QueuePopSound sound, double volume);
    void Dispose();
}

public class SoundPlayerService : ISoundPlayerService
{
    - MediaPlayer instance
    - Play sound with volume control (0-100%)
    - Handle file paths from Assets/sounds/
    - Thread-safe playback
}
```

**Dependencies**:
- `System.Windows.Media.MediaPlayer` (already available in WPF)
- No additional NuGet packages needed

### 1.2 Configuration Model

**File**: `BlueMeter.WPF/Config/AppConfig.cs`

Add new properties:
```csharp
/// <summary>
/// Enable queue pop sound alerts
/// </summary>
[ObservableProperty]
private bool _queuePopSoundEnabled = true;

/// <summary>
/// Selected queue pop sound
/// </summary>
[ObservableProperty]
private QueuePopSound _queuePopSound = QueuePopSound.Harp;

/// <summary>
/// Queue pop sound volume (0-100%)
/// </summary>
[ObservableProperty]
private double _queuePopSoundVolume = 10.0;
```

**File**: `BlueMeter.WPF/Models/QueuePopSound.cs`

```csharp
public enum QueuePopSound
{
    Drum,
    Harp,
    Wow,
    Yoooo
}
```

### 1.3 Settings UI - New Tab "Alerts"

**File**: `BlueMeter.WPF/Views/SettingsView.xaml`

Add new tab under Basic Settings:
```xml
<TabItem Header="Alerts">
    <StackPanel>
        <!-- Enable/Disable -->
        <CheckBox Content="Enable Queue Pop Sound"
                  IsChecked="{Binding Config.QueuePopSoundEnabled}"/>

        <!-- Sound Selection -->
        <Label Content="Alert Sound:"/>
        <ComboBox SelectedItem="{Binding Config.QueuePopSound}">
            <ComboBoxItem>Drum</ComboBoxItem>
            <ComboBoxItem>Harp</ComboBoxItem>
            <ComboBoxItem>Wow</ComboBoxItem>
            <ComboBoxItem>Yoooo</ComboBoxItem>
        </ComboBox>

        <!-- Volume Slider -->
        <Label Content="Volume:"/>
        <Slider Minimum="0" Maximum="100"
                Value="{Binding Config.QueuePopSoundVolume}"
                TickFrequency="10" TickPlacement="BottomRight"/>
        <TextBlock Text="{Binding Config.QueuePopSoundVolume, StringFormat={}{0:0}%}"/>

        <!-- Test Button -->
        <Button Content="Test Sound" Command="{Binding TestQueuePopSoundCommand}"/>
    </StackPanel>
</TabItem>
```

**File**: `BlueMeter.WPF/ViewModels/SettingsViewModel.cs`

Add command:
```csharp
[RelayCommand]
private void TestQueuePopSound()
{
    _soundPlayerService.TestSound(
        Config.QueuePopSound,
        Config.QueuePopSoundVolume
    );
}
```

### 1.4 Dependency Injection Setup

**File**: `BlueMeter.WPF/App.xaml.cs`

Register service:
```csharp
services.AddSingleton<ISoundPlayerService, SoundPlayerService>();
```

---

## Phase 2: Packet Analysis & Queue Detection 🔍

### 2.1 Debug Logging for Queue Detection

**Approach**: Log all packets when players connect to identify the queue pop pattern.

**File**: `BlueMeter.Core/Analyze/MessageAnalyzer.cs`

Add debug logging for:
- Team/Party formation packets
- Player connection events
- Matchmaking/dungeon finder packets

**Expected Patterns to Look For**:
1. **Multiple players joining simultaneously** (3-5 players within 1 second)
2. **Team/Party creation message**
3. **Dungeon/Raid instance creation**
4. **Matchmaking completion packet**

### 2.2 Create Debug Command

**File**: `BlueMeter.WPF/ViewModels/DebugFunctions.cs`

Add toggle:
```csharp
[ObservableProperty]
private bool _logQueueDetection = false;

partial void OnLogQueueDetectionChanged(bool value)
{
    // Enable/disable queue detection logging
    DataStorage.EnableQueueDetectionLogging = value;
}
```

### 2.3 Testing Process

**User Action**:
1. Enable "Log Queue Detection" in Debug menu
2. Queue for dungeon/raid
3. When queue pops → Accept
4. Check logs for patterns

**Look for in logs**:
```
[QueueDetection] Team formation detected: 5 players
[QueueDetection] Player joined: UID=12345, Name=PlayerName
[QueueDetection] Dungeon instance created: InstanceID=67890
```

---

## Phase 3: Queue Pop Detection Implementation 🎯

### 3.1 Detection Strategy

Based on Phase 2 findings, implement one of these strategies:

**Strategy A: Player Count Spike Detection**
```csharp
// Detect when 3+ players join within 2 seconds
if (newPlayerCount >= currentPlayerCount + 3
    && timeSinceLastJoin < TimeSpan.FromSeconds(2))
{
    OnQueuePopDetected();
}
```

**Strategy B: Party/Team Formation Packet**
```csharp
// Detect specific packet type for team formation
if (packet.Type == PacketType.TeamFormed
    || packet.Type == PacketType.DungeonMatchFound)
{
    OnQueuePopDetected();
}
```

**Strategy C: Matchmaking Status Change**
```csharp
// Detect status change from "In Queue" to "Match Found"
if (previousStatus == MatchmakingStatus.InQueue
    && currentStatus == MatchmakingStatus.MatchFound)
{
    OnQueuePopDetected();
}
```

### 3.2 Queue Detection Service

**File**: `BlueMeter.WPF/Services/QueueDetectionService.cs`

```csharp
public interface IQueueDetectionService
{
    event EventHandler? QueuePopped;
    void Start();
    void Stop();
}

public class QueueDetectionService : IQueueDetectionService
{
    public event EventHandler? QueuePopped;

    private void OnPlayerCountChanged(int newCount)
    {
        // Implement detection logic from Phase 2 findings
        if (IsQueuePop(newCount))
        {
            QueuePopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

### 3.3 Integration

**File**: `BlueMeter.WPF/Services/ApplicationStartup.cs`

Subscribe to queue detection:
```csharp
_queueDetectionService.QueuePopped += OnQueuePopped;

private void OnQueuePopped(object? sender, EventArgs e)
{
    if (_configManager.CurrentConfig.QueuePopSoundEnabled)
    {
        _soundPlayerService.PlayQueuePopSound();
    }
}
```

---

## Phase 4: Refinements & Polish ✨

### 4.1 Cooldown Prevention

Prevent sound spam if detection triggers multiple times:
```csharp
private DateTime _lastQueuePopTime = DateTime.MinValue;
private readonly TimeSpan _queuePopCooldown = TimeSpan.FromSeconds(30);

if (DateTime.Now - _lastQueuePopTime > _queuePopCooldown)
{
    PlaySound();
    _lastQueuePopTime = DateTime.Now;
}
```

### 4.2 Sound Preview in Settings

Add visual feedback when testing:
```csharp
private bool _isPlayingTestSound;

[RelayCommand]
private async Task TestQueuePopSound()
{
    if (_isPlayingTestSound) return;

    _isPlayingTestSound = true;
    TestButtonText = "Playing...";

    await _soundPlayerService.TestSoundAsync(...);

    TestButtonText = "Test Sound";
    _isPlayingTestSound = false;
}
```

### 4.3 Additional Settings

Add more customization options:
- Multiple alert sounds per event type
- Custom sound files (browse for .mp3)
- Visual notification alongside sound
- Desktop notification (Windows toast)

### 4.4 Localization

Add translations for:
- Alert tab title
- Sound names
- Volume label
- Test button text

---

## Testing Checklist

### Phase 1 Testing
- [ ] Sound files load correctly from Assets folder
- [ ] All 4 sounds play when tested
- [ ] Volume slider adjusts playback volume (0-100%)
- [ ] Settings persist after restart
- [ ] Test button works for all sound selections

### Phase 2 Testing
- [ ] Debug logging enabled/disabled via toggle
- [ ] Queue pop events logged with timestamps
- [ ] Packet patterns identified
- [ ] Player join events captured

### Phase 3 Testing
- [ ] Queue pop detected reliably
- [ ] Sound plays on detection
- [ ] No false positives (normal player joins)
- [ ] Works in different queue types (dungeon, raid, PvP)
- [ ] Cooldown prevents spam

### Phase 4 Testing
- [ ] Visual feedback during test playback
- [ ] Settings UI responsive and intuitive
- [ ] No performance impact during combat
- [ ] Sound plays at correct volume

---

## File Structure

```
BlueMeter/
├── BlueMeter.Assets/
│   └── sounds/
│       ├── drum.mp3
│       ├── harp.mp3
│       ├── wow.mp3
│       └── yoooo.mp3
├── BlueMeter.WPF/
│   ├── Config/
│   │   └── AppConfig.cs (add queue pop settings)
│   ├── Models/
│   │   └── QueuePopSound.cs (new enum)
│   ├── Services/
│   │   ├── SoundPlayerService.cs (new)
│   │   └── QueueDetectionService.cs (Phase 3)
│   ├── ViewModels/
│   │   └── SettingsViewModel.cs (add test command)
│   └── Views/
│       └── SettingsView.xaml (add Alerts tab)
└── BlueMeter.Core/
    └── Analyze/
        └── MessageAnalyzer.cs (Phase 2: add logging)
```

---

## Dependencies

**Phase 1**:
- `System.Windows.Media.MediaPlayer` (built-in WPF)
- No additional packages needed

**Phase 2-3**:
- Existing packet analysis infrastructure
- Existing logging system

**Phase 4** (Optional):
- `Microsoft.Toolkit.Uwp.Notifications` (for Windows toast notifications)

---

## Configuration Example

**After Phase 1**, `config.json` will include:
```json
{
  "Config": {
    "queuePopSoundEnabled": true,
    "queuePopSound": "Harp",
    "queuePopSoundVolume": 10.0
  }
}
```

---

## Implementation Order

1. ✅ **Phase 1** - Sound infrastructure & Settings UI (START HERE)
2. 🔍 **Phase 2** - Debug logging & packet analysis
3. 🎯 **Phase 3** - Queue detection implementation
4. ✨ **Phase 4** - Refinements & polish

---

**Status**: Phase 1 Ready to Implement
**Next Steps**:
1. Create `QueuePopSound.cs` enum
2. Add properties to `AppConfig.cs`
3. Create `SoundPlayerService.cs`
4. Update `SettingsView.xaml` with Alerts tab
5. Add test command to `SettingsViewModel.cs`
6. Register service in DI container
