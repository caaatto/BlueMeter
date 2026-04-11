using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using BlueMeter.Services;

namespace BlueMeter.Tests;

public class UpdateCheckerTests
{
    private readonly Mock<ILogger<UpdateChecker>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

    public UpdateCheckerTests()
    {
        _loggerMock = new Mock<ILogger<UpdateChecker>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
    }

    [Theory]
    [InlineData("1.2.10", "1.2.12", true)]  // Update available
    [InlineData("1.2.12", "1.2.12", false)] // Same version
    [InlineData("1.2.13", "1.2.12", false)] // Current is newer
    [InlineData("1.0.0", "2.0.0", true)]    // Major version update
    [InlineData("1.2.0", "1.3.0", true)]    // Minor version update
    [InlineData("2.0.0", "1.9.9", false)]   // Current major is higher
    public async Task CheckForUpdatesAsync_VersionComparison_ReturnsCorrectUpdateAvailability(
        string currentVersion,
        string latestVersion,
        bool expectedUpdateAvailable)
    {
        // Arrange
        var mockResponse = CreateMockGitHubResponse(latestVersion);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Create a test version of UpdateChecker that uses the specified currentVersion
        var updateChecker = new TestUpdateChecker(_loggerMock.Object, _httpClientFactoryMock.Object, currentVersion);

        // Act
        var result = await updateChecker.CheckForUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(latestVersion, result.LatestVersion);
        Assert.Equal(currentVersion, result.CurrentVersion);
        Assert.Equal(expectedUpdateAvailable, result.IsUpdateAvailable);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WithVersionPrefix_RemovesVPrefix()
    {
        // Arrange
        var mockResponse = CreateMockGitHubResponse("1.2.12", tagName: "v1.2.12");
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var updateChecker = new TestUpdateChecker(_loggerMock.Object, _httpClientFactoryMock.Object, "1.2.10");

        // Act
        var result = await updateChecker.CheckForUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1.2.12", result.LatestVersion); // Should be without 'v' prefix
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WithHttpError_ReturnsNull()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var updateChecker = new UpdateChecker(_loggerMock.Object, _httpClientFactoryMock.Object);

        // Act
        var result = await updateChecker.CheckForUpdatesAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ParsesReleaseUrl()
    {
        // Arrange
        var expectedUrl = "https://github.com/caaatto/BlueMeter/releases/tag/v1.2.12";
        var mockResponse = CreateMockGitHubResponse("1.2.12", releaseUrl: expectedUrl);
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var updateChecker = new TestUpdateChecker(_loggerMock.Object, _httpClientFactoryMock.Object, "1.2.10");

        // Act
        var result = await updateChecker.CheckForUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUrl, result.ReleaseUrl);
    }

    private static HttpResponseMessage CreateMockGitHubResponse(
        string version,
        string? tagName = null,
        string? releaseUrl = null)
    {
        tagName ??= version;
        releaseUrl ??= $"https://github.com/caaatto/BlueMeter/releases/tag/{tagName}";

        var jsonResponse = $@"{{
            ""tag_name"": ""{tagName}"",
            ""html_url"": ""{releaseUrl}"",
            ""name"": ""Release {version}"",
            ""published_at"": ""2024-01-01T00:00:00Z""
        }}";

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };
    }

    // Test subclass to override GetCurrentVersion
    private class TestUpdateChecker : UpdateChecker
    {
        private readonly string _testVersion;

        public TestUpdateChecker(
            ILogger<UpdateChecker> logger,
            IHttpClientFactory httpClientFactory,
            string testVersion)
            : base(logger, httpClientFactory)
        {
            _testVersion = testVersion;
        }

        protected override string GetCurrentVersion()
        {
            return _testVersion;
        }
    }
}
