namespace FtpPlayground;

public class FtpConnectionDetails
{
    public required string Host { get; init; }
    public int Port { get; set; } = 21;
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string BasePath { get; set; } = "";
    public bool UseSsl { get; set; } = false;
    public int? TimeoutSeconds { get; set; }
}