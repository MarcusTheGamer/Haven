namespace Haven.Services;

public class DeviceNetworkSweepService
{
    private static readonly TimeSpan PerHostTimeout = TimeSpan.FromMilliseconds(400);
    private const int MaxConcurrentRequests = 32;
    private readonly IWifiConnector _wifiConnector;

    public DeviceNetworkSweepService(IWifiConnector wifiConnector) => _wifiConnector = wifiConnector;

    public async Task<string?> FindDeviceIpAsync(string expectedDeviceId, TimeSpan? overallTimeout = null, CancellationToken cancellationToken = default)
    {
        var localIpString = await _wifiConnector.GetLocalIpAddressAsync();
        if (string.IsNullOrWhiteSpace(localIpString)) return null;

        var parts = localIpString.Split('.');
        if (parts.Length != 4) return null;

        var subnetPrefix = $"{parts[0]}.{parts[1]}.{parts[2]}.";
        using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(overallTimeout ?? TimeSpan.FromSeconds(20));
        using var throttle = new SemaphoreSlim(MaxConcurrentRequests);

        string? foundIp = null;
        var tasks = new List<Task>();

        for (var host = 1; host <= 254; host++)
        {
            var candidateIp = $"{subnetPrefix}{host}";
            if (candidateIp == localIpString) continue;

            tasks.Add(CheckHostAsync(candidateIp, expectedDeviceId, throttle, overallCts, ip => foundIp = ip));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // sweep was cancelled
        }

        return foundIp;
    }

    // check one host
    private static async Task CheckHostAsync(string ip, string expectedDeviceId, SemaphoreSlim throttle, CancellationTokenSource overallCts, Action<string> onFound)
    {
        var acquired = false;

        try
        {
            await throttle.WaitAsync(overallCts.Token).ConfigureAwait(false);
            acquired = true;
            if (overallCts.IsCancellationRequested) return;

            using var http = new HttpClient { BaseAddress = new Uri($"http://{ip}"), Timeout = PerHostTimeout };
            using var perHostCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
            perHostCts.CancelAfter(PerHostTimeout);

            var info = await http.GetStringAsync("/info", perHostCts.Token).ConfigureAwait(false);
            var parts = info.Split(';');

            if (parts.Length >= 5 &&
                string.Equals(parts[0], "HAVEN_DEVICE", StringComparison.Ordinal) &&
                string.Equals(parts[2], expectedDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                onFound(ip);
                overallCts.Cancel();
            }
        }
        catch (OperationCanceledException)
        {
            // request timed out
        }
        catch
        {
            // host was unreachable
        }
        finally
        {
            if (acquired) throttle.Release();
        }
    }
}