using FluentFTP;

namespace FtpPlayground;

public partial class FtpService
{
    /// <summary>
    /// Minimal adapter interface used for unit testing. Kept internal and small to avoid depending on FluentFTP's large
    /// public API surface when writing fakes.
    /// </summary>
    internal interface IFtpClientAdapter : IAsyncDisposable, IDisposable
    {
        bool IsConnected { get; }
        Task Connect(CancellationToken cancellationToken = default);
        Task Disconnect(CancellationToken cancellationToken = default);
        IAsyncEnumerable<FtpListItem> GetListingEnumerable(string path, CancellationToken cancellationToken = default);
        Task<Stream> OpenRead(string path, FtpDataType type = FtpDataType.Binary, long restartPosition = 0, CancellationToken token = default);
        Task DownloadFile(string localPath, string remotePath, FtpLocalExists existsMode = FtpLocalExists.Overwrite, CancellationToken token = default);
        Task UploadStream(Stream sourceStream, string remotePath, FtpRemoteExists existsMode = FtpRemoteExists.Overwrite, CancellationToken token = default);
        Task<string> GetWorkingDirectory(CancellationToken token = default);
    }
}

