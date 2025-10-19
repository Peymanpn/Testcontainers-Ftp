using FtpPlayground;
using System.Net.Http.Json;

namespace TestProject;

/// <summary>
/// Intentionally duplicated test class to trigger multiple test runs and validate dynamic port mapping 
/// for FTP Server passive mode in the test environment.
/// </summary>
public class Ftptests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public Ftptests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid1()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid2()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid3()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid4()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }

    [Fact]
    public async Task TestLocalEndpoint_ShouldReturnSuccess_WhenFtpConnectionIsValid5()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var connectionDetails = new FtpConnectionDetails
        {
            Host = _fixture.FtpHost,
            Port = _fixture.FtpPort,
            Username = _fixture.FtpUsername,
            Password = _fixture.FtpPassword,
            UseSsl = false,
            BasePath = "/",
            TimeoutSeconds = 5,
        };

        // Act
        var response = await client.PostAsJsonAsync("/ftp/test", connectionDetails, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("\"FTP connection successful.\"", responseContent); // JSON string
    }


}
