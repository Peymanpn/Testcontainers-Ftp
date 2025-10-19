namespace FtpPlayground;


public interface IFtpService
{

    /// <summary>
    /// Asynchronously downloads a file from an FTP server to a specified local path.
    /// </summary>
    /// <remarks>If a file already exists at <paramref name="localPath"/>, it may be overwritten. This method
    /// does not guarantee atomicity; partial files may be left if the download fails. The caller is responsible for
    /// ensuring sufficient permissions for both the FTP server and the local file system.</remarks>
    /// <param name="connectionDetails">The FTP connection details used to authenticate and establish the connection to the remote server. Cannot be
    /// null.</param>
    /// <param name="remoteFilePath">The full path of the file on the FTP server to download. Must be a valid path and cannot be null or empty.</param>
    /// <param name="localPath">The local file system path where the downloaded file will be saved. Must be a valid path and cannot be null or
    /// empty.</param>
    /// <returns>A task that represents the asynchronous download operation. The task completes when the file has been
    /// successfully downloaded.</returns>
    Task DownloadFileAsync(FtpConnectionDetails connectionDetails, string remoteFilePath, string localPath);

    /// <summary>
    /// Asynchronously uploads a file to the specified remote path on an FTP server.
    /// </summary>
    /// <remarks>If the remote file already exists, it may be overwritten depending on server configuration.
    /// The method does not close or dispose the provided stream after completion. This method is thread-safe and can be
    /// called concurrently for different uploads.</remarks>
    /// <param name="connectionDetails">The connection information required to access the target FTP server, including host, credentials, and port.</param>
    /// <param name="remoteFilePath">The full path on the FTP server where the file will be uploaded. Must be a valid path and include the desired
    /// filename.</param>
    /// <param name="sourceStream">The stream containing the file data to upload. The stream must be readable and positioned at the start of the
    /// data to transfer.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the upload operation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous upload operation. The task completes when the file has been successfully
    /// uploaded or the operation is canceled.</returns>
    Task UploadFileAsync(FtpConnectionDetails connectionDetails, string remoteFilePath, Stream sourceStream, CancellationToken cancellationToken = default);


    /// <summary>
    /// Attempts to establish a connection to the specified FTP server and returns the result of the connection test.
    /// </summary>
    /// <remarks>This method does not throw exceptions for connection failures; instead, error information is
    /// provided in the returned tuple. The method is thread-safe and can be called concurrently from multiple
    /// threads.</remarks>
    /// <param name="connectionDetails">The FTP server connection details, including host, credentials, and port information. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the connection attempt. Optional.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains a boolean indicating whether the
    /// connection was successful, and an error message if the connection failed; otherwise, null.</returns>
    Task<(bool IsSuccess, string? ErrorMessage)> TestConnection(FtpConnectionDetails connectionDetails, CancellationToken cancellationToken = default);
}

