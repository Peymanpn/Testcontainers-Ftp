using FluentFTP;
using FluentFTP.Exceptions;
using System.Net;
using System.Text;

namespace FtpPlayground;


public partial class FtpService : IFtpService
{
    private readonly ILogger<FtpService> _logger;
    private readonly Func<FtpConnectionDetails, IFtpClientAdapter> _clientFactory;

    public FtpService(ILogger<FtpService> logger)
        : this(logger, null)
    {
    }

    // Internal-friendly constructor for tests: allows injection of a test client adapter factory.
    internal FtpService(ILogger<FtpService> logger, Func<FtpConnectionDetails, IFtpClientAdapter>? clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory ?? (details => new FluentFtpClientAdapter(ConfigureClient(details)));
    }



    public async Task DownloadFileAsync(FtpConnectionDetails connectionDetails, string remoteFilePath, string localPath)
    {
        try
        {
            using var client = _clientFactory(connectionDetails);

            string basePath = connectionDetails.BasePath.TrimStart('/').TrimEnd('/');
            string fullRemotePath = string.IsNullOrEmpty(basePath)
                ? remoteFilePath.TrimStart('/')
                : $"{basePath}/{remoteFilePath.TrimStart('/')}";

            _logger.LogDebug("Downloading remote file '{RemotePath}' to local path '{LocalPath}'", fullRemotePath, localPath);

            await client.Connect();

            await client.DownloadFile(localPath: localPath, remotePath: fullRemotePath, existsMode: FtpLocalExists.Overwrite);

            await client.Disconnect();
            _logger.LogInformation("File downloaded to {LocalPath}", localPath);
        }
        catch (FtpCommandException ex)
        {
            _logger.LogError(ex, "FTP error while downloading file: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while downloading file: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UploadFileAsync(FtpConnectionDetails connectionDetails, string remoteFilePath, Stream sourceStream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _clientFactory(connectionDetails);

            string basePath = connectionDetails.BasePath.TrimStart('/').TrimEnd('/');
            string fullRemotePath = string.IsNullOrEmpty(basePath)
                ? remoteFilePath.TrimStart('/')
                : $"{basePath}/{remoteFilePath.TrimStart('/')}";

            _logger.LogDebug("Uploading to remote file '{RemotePath}' from stream", fullRemotePath);

            await client.Connect(cancellationToken);

            await client.UploadStream(sourceStream, fullRemotePath, existsMode: FtpRemoteExists.Overwrite, token: cancellationToken);

            await client.Disconnect(cancellationToken);
            _logger.LogInformation("File uploaded to {RemotePath}", fullRemotePath);
        }
        catch (FtpCommandException ex)
        {
            _logger.LogError(ex, "FTP error while uploading file: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while uploading file: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> TestConnection(
        FtpConnectionDetails connectionDetails, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing FTP connection to host {Host}...", connectionDetails.Host);

        using var client = _clientFactory(connectionDetails);

        try
        {
            await client.Connect(cancellationToken);
            await client.GetWorkingDirectory(cancellationToken);

            _logger.LogInformation("FTP connection test successful for host {Host}.", connectionDetails.Host);
            return (true, "Connection successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP connection test failed for host {Host}.", connectionDetails.Host);
            return (false, ex.Message);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.Disconnect(cancellationToken);
            }
        }
    }

    public async Task<List<string>> ReadTopLinesAsync(
       FtpConnectionDetails connectionDetails,
       string remotePath, int lineCount, CancellationToken cancellationToken = default)
    {
        var lines = new List<string>();

        using var client = _clientFactory(connectionDetails);

        try
        {
            await client.Connect(cancellationToken);
            string fullRemotePath = BuildFullPath(connectionDetails.BasePath, remotePath);

            await using (Stream ftpStream = await client.OpenRead(fullRemotePath, FtpDataType.Binary, 0, token: cancellationToken))
            {
                using (var reader = new StreamReader(ftpStream, Encoding.UTF8))
                {
                    for (int i = 0; i < lineCount; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        string? line = await reader.ReadLineAsync();
                        if (line is null) break;
                        lines.Add(line);
                    }
                }
            }
            _logger.LogInformation("Successfully read {LineCount} lines from the file.", lines.Count);
            return lines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read lines from {RemotePath}. {Error}", remotePath, ex.Message);
            throw;
        }
    }



    private static AsyncFtpClient ConfigureClient(FtpConnectionDetails connectionDetails)
    {
        var client = new AsyncFtpClient
        {
            Host = connectionDetails.Host,
            Port = connectionDetails.Port,
            Credentials = new NetworkCredential(connectionDetails.Username, connectionDetails.Password),
        };
        if (connectionDetails.UseSsl)
        {
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
            client.Config.ValidateAnyCertificate = true;
        }

        if (connectionDetails.TimeoutSeconds.HasValue)
        {
            int timeoutMilliseconds = connectionDetails.TimeoutSeconds.Value * 1000;
            client.Config.ConnectTimeout = timeoutMilliseconds;
            client.Config.ReadTimeout = timeoutMilliseconds;
            client.Config.DataConnectionConnectTimeout = timeoutMilliseconds;
        }
        return client;
    }

    private static string BuildFullPath(string? basePath, string? remotePath)
    {
        string safeBasePath = basePath?.TrimStart('/')?.TrimEnd('/') ?? string.Empty;
        string fullRemotePath;

        if (remotePath is null)
        {
            fullRemotePath = string.IsNullOrEmpty(safeBasePath) ? "/" : $"/{safeBasePath}";
        }
        else
        {
            fullRemotePath = string.IsNullOrEmpty(safeBasePath)
                ? remotePath.TrimStart('/')
                : $"{safeBasePath}/{remotePath.TrimStart('/')}";
        }

        return fullRemotePath;
    }
}
