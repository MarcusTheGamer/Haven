using Models = Haven.Models;

namespace Haven.Services;

public class WiFiProvisioningService
{
    public const string DefaultDeviceIp = "192.168.4.1";

    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(5);

    private readonly IWifiConnector _wifiConnector;

    public WiFiProvisioningService(IWifiConnector wifiConnector)
    {
        _wifiConnector = wifiConnector;
    }

    public async Task<string?> GetCurrentHomeSsidAsync()
    {
        Console.WriteLine("[WiFiProvisioningService] Reading current Wi-Fi SSID...");

        var ssid = await _wifiConnector.GetCurrentSsidAsync();

        Console.WriteLine($"[WiFiProvisioningService] Current SSID: {ssid ?? "(none)"}");

        return ssid;
    }

    public async Task<List<string>> DiscoverDevicesAsync()
    {
        Console.WriteLine(
            "[WiFiProvisioningService] Discovering HAVEN devices...");

        var networks =
            await _wifiConnector.ScanForHavenNetworksAsync();

        Console.WriteLine(
            $"[WiFiProvisioningService] Found {networks.Count} HAVEN device(s).");

        return networks;
    }

    public async Task<bool> ConnectToDeviceApAsync(
        string ssid,
        string password = "haven1234")
    {
        Console.WriteLine(
            $"[WiFiProvisioningService] Connecting to {ssid}...");

        var connected =
            await _wifiConnector.ConnectToNetworkAsync(
                ssid,
                password);

        Console.WriteLine(
            $"[WiFiProvisioningService] Connection result: {connected}");

        return connected;
    }

    public void DisconnectFromDeviceAp()
    {
        Console.WriteLine(
            "[WiFiProvisioningService] Disconnecting from device AP.");

        _wifiConnector.DisconnectAndRestoreHomeWifi();
    }

    public async Task<Models.DeviceInfo?> ReadDeviceInfoAsync(
        string ipAddress = DefaultDeviceIp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] GET http://{ipAddress}/info");

            using var http = CreateClient(ipAddress);

            var info = await http.GetStringAsync(
                "/info",
                cancellationToken);

            Console.WriteLine(
                $"[WiFiProvisioningService] /info returned: {info}");

            var parts = info.Split(';');

            if (parts.Length < 5 ||
                !string.Equals(
                    parts[0],
                    "HAVEN_DEVICE",
                    StringComparison.Ordinal))
            {
                Console.WriteLine(
                    "[WiFiProvisioningService] Invalid device info response.");

                return null;
            }

            return new Models.DeviceInfo
            {
                Id = parts[2],
                Type = parts[3],
                Name = parts[4]
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] ReadDeviceInfoAsync failed: {ex}");

            return null;
        }
    }

    public async Task<bool> ProvisionDeviceAsync(
        string ssid,
        string password,
        string ipAddress = DefaultDeviceIp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] POST http://{ipAddress}/provision");

            using var http = CreateClient(ipAddress);

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(
                    "ssid",
                    ssid),

                new KeyValuePair<string, string>(
                    "password",
                    password)
            });

            var response = await http.PostAsync(
                "/provision",
                form,
                cancellationToken);

            Console.WriteLine(
                $"[WiFiProvisioningService] /provision returned {(int)response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] ProvisionDeviceAsync failed: {ex}");

            return false;
        }
    }

    public async Task<string?> GetStatusAsync(
        string ipAddress = DefaultDeviceIp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] GET http://{ipAddress}/status");

            using var http = CreateClient(ipAddress);

            return await http.GetStringAsync(
                "/status",
                cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WiFiProvisioningService] GetStatusAsync failed: {ex}");

            return null;
        }
    }

    private static HttpClient CreateClient(string ipAddress)
    {
        return new HttpClient
        {
            BaseAddress = new Uri($"http://{ipAddress}"),
            Timeout = RequestTimeout
        };
    }
}

public interface IWifiConnector
{
    Task<List<string>> ScanForHavenNetworksAsync();
    Task<bool> ConnectToNetworkAsync(string ssid, string password);
    void DisconnectAndRestoreHomeWifi();
    Task<string?> GetCurrentSsidAsync();
}