using Models = Haven.Models;

namespace Haven.Services;

public class WiFiProvisioningService
{
    public const string DefaultDeviceIp = "192.168.4.1";

    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ProvisionRequestTimeout = TimeSpan.FromSeconds(8);

    private readonly IWifiConnector _wifiConnector;

    public WiFiProvisioningService(IWifiConnector wifiConnector) => _wifiConnector = wifiConnector;

    public async Task<string?> GetCurrentHomeSsidAsync() => await _wifiConnector.GetCurrentSsidAsync();

    public async Task<List<string>> DiscoverDevicesAsync() => await _wifiConnector.ScanForHavenNetworksAsync();

    public async Task<bool> ConnectToDeviceApAsync(string ssid, string password = "haven1234") =>
        await _wifiConnector.ConnectToNetworkAsync(ssid, password);

    public void DisconnectFromDeviceAp() => _wifiConnector.DisconnectAndRestoreHomeWifi();

    public async Task<Models.DeviceInfo?> ReadDeviceInfoAsync(string ipAddress = DefaultDeviceIp, CancellationToken cancellationToken = default)
    {
        try
        {
            // ask the device who it is
            using var http = CreateClient(ipAddress);
            var info = await http.GetStringAsync("/info", cancellationToken);
            return ParseDeviceInfo(info);
        }
        catch
        {
            return null;
        }
    }

    public async Task<DeviceNetworkResult?> FindDeviceOnHomeNetworkAsync(string? expectedDeviceId = null, CancellationToken cancellationToken = default)
    {
        // get the phone's current LAN address
        var localIp = await _wifiConnector.GetLocalIpAddressAsync();
        if (string.IsNullOrWhiteSpace(localIp)) return null;

        // turn 192.168.1.42 into 192.168.1.x
        var parts = localIp.Split('.');
        if (parts.Length != 4) return null;

        var subnet = $"{parts[0]}.{parts[1]}.{parts[2]}.";

        // stop the entire scan after 15 seconds
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

        using var semaphore = new SemaphoreSlim(16);
        var tasks = new List<Task<DeviceNetworkResult?>>();

        // check every possible device address in the /24 subnet
        for (var host = 1; host <= 254; host++)
            tasks.Add(CheckDeviceIpAsync($"{subnet}{host}", expectedDeviceId, semaphore, timeoutCts.Token));

        // process addresses as soon as each request finishes
        while (tasks.Count > 0)
        {
            var completed = await Task.WhenAny(tasks);
            tasks.Remove(completed);

            var result = await completed;
            if (result == null) continue;

            // device found, stop the remaining requests
            timeoutCts.Cancel();
            return result;
        }

        return null;
    }

    // check whether one IP belongs to our device
    private async Task<DeviceNetworkResult?> CheckDeviceIpAsync(string ip, string? expectedDeviceId, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        // limit how many requests run at once
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var device = await ReadDeviceInfoAsync(ip, cancellationToken);
            if (device == null) return null;

            // ignore other Haven devices when looking for a specific one
            if (!string.IsNullOrWhiteSpace(expectedDeviceId) &&
                !string.Equals(device.Id, expectedDeviceId, StringComparison.OrdinalIgnoreCase))
                return null;

            return new DeviceNetworkResult
            {
                Ip = ip,
                Device = device
            };
        }
        catch
        {
            // most IPs will not have a device
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<bool> ProvisionDeviceAsync(string ssid, string password, string ipAddress = DefaultDeviceIp, CancellationToken cancellationToken = default)
    {
        try
        {
            // provisioning needs extra time for the Arduino to respond
            using var http = CreateClient(ipAddress, ProvisionRequestTimeout);

            using var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ssid", ssid),
                new KeyValuePair<string, string>("password", password)
            });

            using var response = await http.PostAsync("/provision", form, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetStatusAsync(string ipAddress = DefaultDeviceIp, CancellationToken cancellationToken = default)
    {
        try
        {
            using var http = CreateClient(ipAddress);
            return await http.GetStringAsync("/status", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    // convert the device response into a model
    private static Models.DeviceInfo? ParseDeviceInfo(string info)
    {
        var parts = info.Split(';');

        // expected: HAVEN_DEVICE;version;id;type;name
        if (parts.Length < 5 || !string.Equals(parts[0], "HAVEN_DEVICE", StringComparison.Ordinal))
            return null;

        return new Models.DeviceInfo
        {
            Id = parts[2],
            Type = parts[3],
            Name = parts[4]
        };
    }

    // create an HTTP client for a device
    private static HttpClient CreateClient(string ipAddress) => CreateClient(ipAddress, RequestTimeout);

    private static HttpClient CreateClient(string ipAddress, TimeSpan timeout) => new()
    {
        BaseAddress = new Uri($"http://{ipAddress}"),
        Timeout = timeout
    };
}

public class DeviceNetworkResult
{
    public string Ip { get; set; } = string.Empty;
    public Models.DeviceInfo Device { get; set; } = null!;
}

public interface IWifiConnector
{
    Task<List<string>> ScanForHavenNetworksAsync();
    Task<bool> ConnectToNetworkAsync(string ssid, string password);
    void DisconnectAndRestoreHomeWifi();
    Task<string?> GetCurrentSsidAsync();
    Task<string?> GetLocalIpAddressAsync();
}