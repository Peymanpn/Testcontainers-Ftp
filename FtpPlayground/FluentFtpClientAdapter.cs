using FluentFTP;

namespace FtpPlayground;

public partial class FtpService
{
    /// <summary>
    /// Provides an adapter for the <see cref="AsyncFtpClient"/> class, implementing the <see cref="IFtpClientAdapter"/>
    /// interface.
    /// </summary>
    /// <remarks>This class acts as a wrapper around the <see cref="AsyncFtpClient"/> to provide a consistent
    /// interface for FTP operations. It delegates all operations to the underlying <see cref="AsyncFtpClient"/>
    /// instance.</remarks>
    private sealed class FluentFtpClientAdapter : IFtpClientAdapter
    {
        private readonly AsyncFtpClient _inner;

        public FluentFtpClientAdapter(AsyncFtpClient inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool IsConnected => _inner.IsConnected;

        public Task Connect(CancellationToken cancellationToken = default) => _inner.Connect(cancellationToken);
        public Task Disconnect(CancellationToken cancellationToken = default) => _inner.Disconnect(cancellationToken);
        public IAsyncEnumerable<FtpListItem> GetListingEnumerable(string path, CancellationToken cancellationToken = default) => _inner.GetListingEnumerable(path, cancellationToken);
        public Task<Stream> OpenRead(string path, FtpDataType type = FtpDataType.Binary, long restartPosition = 0, CancellationToken token = default) => _inner.OpenRead(path, type, restartPosition, token: token);
        public Task DownloadFile(string localPath, string remotePath, FtpLocalExists existsMode = FtpLocalExists.Overwrite, CancellationToken token = default) => _inner.DownloadFile(localPath: localPath, remotePath: remotePath, existsMode: existsMode, token: token);
        public Task UploadStream(Stream sourceStream, string remotePath, FtpRemoteExists existsMode = FtpRemoteExists.Overwrite, CancellationToken token = default) => _inner.UploadStream(fileStream: sourceStream, remotePath: remotePath, existsMode: existsMode, token: token);
        public Task<string> GetWorkingDirectory(CancellationToken token = default) => _inner.GetWorkingDirectory(token);

        public void Dispose() => _inner.Dispose();

        public ValueTask DisposeAsync() => _inner.DisposeAsync();
    }
}

