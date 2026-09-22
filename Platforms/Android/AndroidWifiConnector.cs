#if ANDROID
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Haven.Services;

namespace Haven.Platforms.Android;

public class AndroidWifiConnector : IWifiConnector
{
    private ConnectivityManager? _connectivityManager;
    private ConnectivityManager.NetworkCallback? _networkCallback;

    public async Task<List<string>> ScanForHavenNetworksAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("[WiFi] Location permission was not granted.");
                return new List<string>();
            }
        }

        var context = global::Android.App.Application.Context;
        var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService)!;

        if (!wifiManager.IsWifiEnabled)
        {
            Console.WriteLine("[WiFi] Wi-Fi is disabled.");
            return new List<string>();
        }

        Console.WriteLine("[WiFi] Starting Wi-Fi scan...");

        var scanStarted = wifiManager.StartScan();

        Console.WriteLine($"[WiFi] StartScan result: {scanStarted}");

        if (!scanStarted)
        {
            Console.WriteLine("[WiFi] StartScan failed. Using cached results.");
            return GetHavenNetworks(wifiManager.ScanResults);
        }

        await Task.Delay(2000);

        var networks = GetHavenNetworks(wifiManager.ScanResults);

        Console.WriteLine($"[WiFi] Found {networks.Count} HAVEN network(s).");

        foreach (var network in networks)
            Console.WriteLine($"[WiFi] {network}");

        return networks;
    }

    private static List<string> GetHavenNetworks(
        IEnumerable<ScanResult>? scanResults)
    {
        if (scanResults == null)
            return new List<string>();

        return scanResults
            .Where(r => !string.IsNullOrWhiteSpace(r.Ssid))
            .Select(r => r.Ssid)
            .Where(ssid =>
                ssid.StartsWith(
                    "HAVEN-",
                    StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public Task<bool> ConnectToNetworkAsync(
        string ssid,
        string password)
    {
        var tcs = new TaskCompletionSource<bool>();

        var context = global::Android.App.Application.Context;

        _connectivityManager =
            (ConnectivityManager)context.GetSystemService(
                Context.ConnectivityService)!;

        if (_networkCallback != null)
        {
            try
            {
                _connectivityManager.UnregisterNetworkCallback(
                    _networkCallback);
            }
            catch
            {
            }

            _networkCallback = null;
        }

        Console.WriteLine(
            $"[WiFi] Requesting device network: {ssid}");

        var specifier = new WifiNetworkSpecifier.Builder()
            .SetSsid(ssid)
            .SetWpa2Passphrase(password)
            .Build();

        var request = new NetworkRequest.Builder()
            .AddTransportType(TransportType.Wifi)
            .SetNetworkSpecifier(specifier)
            .Build();

        _networkCallback = new ProvisioningNetworkCallback(
            onAvailable: network =>
            {
                Console.WriteLine(
                    "[WiFi] Device network available.");

                var bound =
                    _connectivityManager.BindProcessToNetwork(network);

                Console.WriteLine(
                    $"[WiFi] BindProcessToNetwork result: {bound}");

                tcs.TrySetResult(bound);
            },
            onUnavailable: () =>
            {
                Console.WriteLine(
                    "[WiFi] Device network unavailable.");

                tcs.TrySetResult(false);
            });

        try
        {
            _connectivityManager.RequestNetwork(
                request,
                _networkCallback);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WiFi] RequestNetwork failed: {ex}");

            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    public void DisconnectAndRestoreHomeWifi()
    {
        Console.WriteLine(
            "[WiFi] Disconnecting from device network.");

        if (_connectivityManager == null)
            return;

        try
        {
            _connectivityManager.BindProcessToNetwork(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WiFi] Failed to unbind process: {ex.Message}");
        }

        if (_networkCallback != null)
        {
            try
            {
                _connectivityManager.UnregisterNetworkCallback(
                    _networkCallback);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[WiFi] Failed to unregister callback: {ex.Message}");
            }

            _networkCallback = null;
        }
    }

    private class ProvisioningNetworkCallback :
        ConnectivityManager.NetworkCallback
    {
        private readonly Action<Network> _onAvailable;
        private readonly Action _onUnavailable;

        public ProvisioningNetworkCallback(
            Action<Network> onAvailable,
            Action onUnavailable)
        {
            _onAvailable = onAvailable;
            _onUnavailable = onUnavailable;
        }

        public override void OnAvailable(Network network)
        {
            base.OnAvailable(network);
            _onAvailable(network);
        }

        public override void OnUnavailable()
        {
            base.OnUnavailable();
            _onUnavailable();
        }
    }
    public async Task<string?> GetCurrentSsidAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("[WiFi] Location permission was not granted.");
                return null;
            }
        }

        var context = global::Android.App.Application.Context;
        var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService)!;

        var ssid = wifiManager.ConnectionInfo?.SSID;

        if (string.IsNullOrWhiteSpace(ssid) || ssid == "<unknown ssid>")
            return null;

        return ssid.Trim('"');
    }
}
#endif