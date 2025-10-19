using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentFTP;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using Xunit.Internal;

namespace TestProject;

/// <summary>
/// Provides a test fixture for integration testing of the web application, including automatic setup and management of
/// an isolated FTP server container with unique credentials and reserved ports.
/// </summary>
/// <remarks>
/// This fixture configures and starts a dedicated FTP server container for each test run, ensuring that
/// FTP credentials and port assignments are unique and isolated. 
/// 
/// It reserves a range of consecutive ports for passive FTP mode to avoid conflicts with other tests or processes. 
/// The fixture implements asynchronous initialization and disposal to manage container lifecycle and resource cleanup. 
/// Use this class to enable reliable end-to-end testing scenarios that require FTP interactions alongside the web application.
/// </remarks>
public class WebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const int ConsecutiveFreePortsNeeded = 5;
    private const int PassivePortRangeStart = 30000;
    private const int PassivePortRangeEnd = 30999;
    private const int MaxRetries = 10;
    private const int RetryDelayMs = 5500;
    private const string TestFileName = "debug.txt";

    private readonly IContainer _ftpContainer;
    public int FtpPort => _ftpContainer.GetMappedPublicPort(21);
    public string FtpUsername { get; private set; }
    public string FtpPassword { get; private set; }
    public string FtpHost => "localhost";

    public Guid ApiKey { get; private set; }
    public Guid TenantId { get; internal set; }

    private readonly object _lock = new();  // For batch reservation
    private (int Start, int End)? _passivePortRange;  // Track for disposal
    private static HashSet<int>? _cachedUsedPorts;
    private readonly static ConcurrentDictionary<int, bool> _reservedPorts = new();

    public WebAppFixture()
    {
        FtpUsername = Guid.NewGuid().ToString();
        FtpPassword = Guid.NewGuid().ToString();

        // create default container builder
        var containerBuilder = new ContainerBuilder()
            .WithImage("stilliard/pure-ftpd:hardened")  // Reliable FTP image
            .WithPortBinding(21, true)  // Expose FTP port
            .WithEnvironment("FTP_USER_NAME", FtpUsername)
            .WithEnvironment("FTP_USER_PASS", FtpPassword)
            .WithEnvironment("FTP_USER_HOME", $"/home/{FtpUsername}")
            .WithEnvironment("PUBLICHOST", "localhost")  // For passive mode
            .WithEnvironment("ADDED_FLAGS", "-d -d") // Disable TLS for testing
            ;

        // Configure passive ports with reservation
        containerBuilder = ConfigurePassivePorts(containerBuilder);

        // Build the container
        _ftpContainer = containerBuilder.Build();
    }

    /// <summary>
    /// Attempts to reserve the specified network port for exclusive use within the application.
    /// </summary>
    public static bool ReservePort(int port)
    {
        return _reservedPorts.TryAdd(port, true);
    }

    /// <summary>
    /// Removes the specified port number from the set of reserved ports, allowing it to be reused for future
    /// operations.
    /// </summary>
    public static void RemoveReservedPort(int port)
    {
        _reservedPorts.TryRemove(port, out _);
    }


    /// <summary>
    /// Configures the specified container builder to use a reserved range of consecutive passive ports for FTP
    /// connections.
    /// </summary>
    private ContainerBuilder ConfigurePassivePorts(ContainerBuilder builder)
    {
        var portRange = FindConsecutiveFreePortRange();  // Now includes batch reservation

        builder = builder.WithEnvironment("FTP_PASSIVE_PORTS", $"{portRange.Start}:{portRange.End}");

        for (int port = portRange.Start; port <= portRange.End; port++)
        {
            builder = builder.WithPortBinding(port, port);
        }
        return builder;
    }

    /// <summary>
    /// Finds and reserves a consecutive range of free TCP ports within the configured passive port range.
    /// </summary>
    /// <remarks>
    /// This method scans the configured passive port range to locate a block of consecutive free ports. 
    /// 
    /// If a suitable range is found, the ports are reserved atomically to prevent race conditions. 
    /// The search may wrap around the port range if necessary, and may briefly wait to allow ports to become available.
    /// </remarks>
    /// <returns>A tuple containing the start and end port numbers of the reserved consecutive free port range.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no consecutive free port range of the required size is available after scanning the entire configured
    /// range.</exception>
    private (int Start, int End) FindConsecutiveFreePortRange()
    {
        int start = PassivePortRangeStart;
        bool wrapped = false;

        while (!wrapped || start <= PassivePortRangeEnd - (ConsecutiveFreePortsNeeded - 1))
        {
            if (start > PassivePortRangeEnd - (ConsecutiveFreePortsNeeded - 1))
            {
                start = PassivePortRangeStart;
                wrapped = true;
                _cachedUsedPorts = null;  // Re-scan after wrap
                Thread.Sleep(100);  // Allow ports to free
            }

            // Use cached or fresh snapshot for candidate checks
            _cachedUsedPorts ??= new HashSet<int>(
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(ep => ep.Port)
            );

            // Check candidate range availability (no reservation yet)
            if (AreAllPortsAvailableInRange(start, ConsecutiveFreePortsNeeded, _cachedUsedPorts))
            {
                // Found candidate: Now batch-reserve atomically
                var range = TryBatchReservePorts(start, ConsecutiveFreePortsNeeded);
                if (range.HasValue)
                {
                    _passivePortRange = range.Value;
                    return range.Value;
                }
                // Else, race loss—continue search
            }

            start++;
        }
        throw new InvalidOperationException("No consecutive free ports found after full scan.");
    }

    /// <summary>
    /// Determines whether all ports within a specified range are available, based on a set of used ports.
    /// </summary>
    /// <param name="startPort">The first port number in the range to check. Must be a non-negative integer.</param>
    /// <param name="count">The number of consecutive ports to check, starting from <paramref name="startPort"/>. Must be greater than zero.</param>
    /// <param name="usedPorts">A set containing port numbers that are currently in use. Ports present in this set are considered unavailable.</param>
    /// <returns>true if none of the ports in the specified range are present in <paramref name="usedPorts"/>; otherwise, false.</returns>
    private static bool AreAllPortsAvailableInRange(int startPort, int count, HashSet<int> usedPorts)
    {
        for (int offset = 0; offset < count; offset++)
        {
            if (usedPorts.Contains(startPort + offset))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to atomically reserve a contiguous batch of TCP ports starting at the specified port number.
    /// </summary>
    /// <remarks>This method ensures that all ports in the specified range are available and reserves them as
    /// a single atomic operation. If any port in the range is unavailable or cannot be reserved, no ports are reserved
    /// and the method returns null. This operation is thread-safe.</remarks>
    /// <param name="startPort">The first port number in the range to reserve. Must be a valid, available TCP port.</param>
    /// <param name="count">The number of consecutive ports to reserve. Must be greater than zero.</param>
    /// <returns>A tuple containing the start and end port numbers of the reserved range if successful; otherwise, null if the
    /// ports could not be reserved.</returns>
    private (int Start, int End)? TryBatchReservePorts(int startPort, int count)
    {
        lock (_lock)  // Atomic batch
        {
            // Re-verify with fresh snapshot (critical for race)
            var freshUsedPorts = new HashSet<int>(
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(ep => ep.Port)
            );

            if (!AreAllPortsAvailableInRange(startPort, count, freshUsedPorts))
            {
                return null;  // Race: Ports no longer free
            }

            // All still free: Batch reserve all
            var reserved = new List<int>();
            bool allReserved = true;
            for (int offset = 0; offset < count; offset++)
            {
                int port = startPort + offset;
                if (!ReservePort(port))
                {
                    allReserved = false;
                    break;  // Abort batch
                }
                reserved.Add(port);
            }

            if (!allReserved)
            {
                // Rollback partial reservations
                foreach (int port in reserved)
                {
                    RemoveReservedPort(port);
                }
                return null;
            }

            int end = startPort + count - 1;
            return (startPort, end);
        }
    }

    /// <summary>
    /// Asynchronously waits until the FTP container is ready to accept connections, retrying the connection test up to
    /// the configured maximum number of attempts.
    /// </summary>
    /// <remarks>This method repeatedly tests the FTP connection, waiting for a short delay between attempts.
    /// The maximum number of retries and the delay between attempts are determined by the configuration. 
    /// This method should be used before performing operations that require the FTP container to be available.</remarks>
    /// <returns>A task that represents the asynchronous wait operation. The task completes when the FTP container is ready.</returns>
    /// <exception cref="TimeoutException">Thrown if the FTP container does not become ready within the allowed number of retries.</exception>
    private async Task WaitForFtpReadyAsync()
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                await TestFtpConnection();
                return; // Ready!
            }
            catch (Exception)
            {
                await Task.Delay(RetryDelayMs);
            }
        }

        throw new TimeoutException("FTP container did not become ready in time.");
    }

    /// <summary>
    /// Tests the ability to connect to the configured FTP server, upload a file, and verify file integrity by
    /// downloading and comparing its contents.
    /// </summary>
    /// <remarks>This method performs a round-trip file upload and download to validate FTP connectivity and
    /// data integrity. It uses the current test context's cancellation token to support cancellation. The method is
    /// intended for use in automated test scenarios and cleans up any temporary files created during
    /// execution.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    private async Task TestFtpConnection()
    {
        var token = TestContext.Current.CancellationToken;

        var client = new AsyncFtpClient
        {
            Host = FtpHost,
            Port = FtpPort,
            Credentials = new NetworkCredential(FtpUsername, FtpPassword),
        };

        // Set timeouts (in ms; adjust as needed; prevents hangs)
        client.Config.ConnectTimeout = 1000;
        client.Config.ReadTimeout = 3000;

        await client.Connect(token);
        var workingDirectory = await client.GetWorkingDirectory(token);

        await File.WriteAllTextAsync(TestFileName, "here", token);
        await client.UploadFile(TestFileName, TestFileName, FtpRemoteExists.Overwrite, false, FtpVerify.None, token: token);

        var tempFile = Path.GetTempFileName();
        try
        {
            await client.DownloadFile(tempFile, TestFileName, FtpLocalExists.Overwrite, FtpVerify.None, token: token);
            var text = await File.ReadAllTextAsync(tempFile, token);
            Assert.Equal("here", text);  // Verification
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(TestFileName)) File.Delete(TestFileName);
        }

        client.Dispose();  // Clean up
    }

    public async ValueTask InitializeAsync()
    {
        await _ftpContainer.StartAsync();
        await WaitForFtpReadyAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        if (_passivePortRange.HasValue)
        {
            for (int port = _passivePortRange.Value.Start; port <= _passivePortRange.Value.End; port++)
            {
                RemoveReservedPort(port);
            }
            _passivePortRange = null;
        }

        RemoveReservedPort(FtpPort);
        _ftpContainer.GetMappedPublicPorts()
            .ForEach(port =>
            {
                RemoveReservedPort(port.Value);
            });


        await _ftpContainer.DisposeAsync();  // Stops and disposes
    }
}