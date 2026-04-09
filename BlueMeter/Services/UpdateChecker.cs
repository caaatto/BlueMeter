using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

public interface IUpdateChecker
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
}

public record UpdateInfo(string LatestVersion, string CurrentVersion, bool IsUpdateAvailable, string ReleaseUrl);

public class UpdateChecker : IUpdateChecker
{
    private readonly ILogger<UpdateChecker> _logger;
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/caaatto/BlueMeter/releases/latest";

    public UpdateChecker(ILogger<UpdateChecker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BlueMeter");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            _logger.LogInformation("Checking for updates. Current version: {Version}", currentVersion);

            var response = await _httpClient.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var latestVersionTag = root.GetProperty("tag_name").GetString();
            if (string.IsNullOrEmpty(latestVersionTag))
            {
                _logger.LogWarning("Could not parse latest version from GitHub");
                return null;
            }

            // Remove 'v' prefix if present (e.g., "v1.2.12" -> "1.2.12")
            var latestVersion = latestVersionTag.TrimStart('v');
            var releaseUrl = root.GetProperty("html_url").GetString() ?? "";

            var isUpdateAvailable = IsNewerVersion(currentVersion, latestVersion);

            _logger.LogInformation(
                "Update check complete. Latest: {Latest}, Current: {Current}, Update available: {Available}",
                latestVersion, currentVersion, isUpdateAvailable);

            return new UpdateInfo(latestVersion, currentVersion, isUpdateAvailable, releaseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates");
            return null;
        }
    }

    protected virtual string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.0.0";
    }

    protected virtual bool IsNewerVersion(string current, string latest)
    {
        try
        {
            var currentParts = current.Split('.');
            var latestParts = latest.Split('.');

            for (int i = 0; i < Math.Max(currentParts.Length, latestParts.Length); i++)
            {
                var currentPart = i < currentParts.Length && int.TryParse(currentParts[i], out var cp) ? cp : 0;
                var latestPart = i < latestParts.Length && int.TryParse(latestParts[i], out var lp) ? lp : 0;

                if (latestPart > currentPart) return true;
                if (latestPart < currentPart) return false;
            }

            return false; // Versions are equal
        }
        catch
        {
            return false;
        }
    }
}
