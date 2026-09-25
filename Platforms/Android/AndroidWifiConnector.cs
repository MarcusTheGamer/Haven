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

    private const int ConnectTimeoutMs = 20000;

    public async Task<List<string>> ScanForHavenNetworksAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                return new List<string>();
            }
        }

        var context = global::Android.App.Application.Context;
        var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService)!;

        if (!wifiManager.IsWifiEnabled)
        {
            return new List<string>();
        }

        var scanStarted = wifiManager.StartScan();

        if (!scanStarted)
        {
            return GetHavenNetworks(wifiManager.ScanResults);
        }

        await Task.Delay(2000);

        var networks = GetHavenNetworks(wifiManager.ScanResults);

        return networks;
    }

    private static List<string> GetHavenNetworks(IEnumerable<ScanResult>? scanResults)
    {
        if (scanResults == null)
            return new List<string>();

        return scanResults
            .Where(r => !string.IsNullOrWhiteSpace(r.Ssid))
            .Select(r => r.Ssid)
            .Where(ssid => ssid.StartsWith("HAVEN-", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public Task<bool> ConnectToNetworkAsync(string ssid, string password)
    {
        var tcs = new TaskCompletionSource<bool>();

        var context = global::Android.App.Application.Context;

        _connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService)!;

        if (_networkCallback != null)
        {
            try
            {
                _connectivityManager.UnregisterNetworkCallback(_networkCallback);
            }
            catch
            {
            }

            _networkCallback = null;
        }

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
                try
                {
                    var bound = _connectivityManager.BindProcessToNetwork(network);

                    tcs.TrySetResult(bound);
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(false);
                }
            },
            onUnavailable: () =>
            {
                tcs.TrySetResult(false);
            });

        try
        {
            _connectivityManager.RequestNetwork(request, _networkCallback, ConnectTimeoutMs);
        }
        catch (Exception ex)
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    public void DisconnectAndRestoreHomeWifi()
    {
        if (_connectivityManager == null)
            return;

        _connectivityManager.BindProcessToNetwork(null);

        if (_networkCallback != null)
        {
            _connectivityManager.UnregisterNetworkCallback(_networkCallback);

            _networkCallback = null;
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

    public Task<string?> GetLocalIpAddressAsync()
    {
        try
        {
            var context = global::Android.App.Application.Context;

            var connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService)!;

            var activeNetwork = connectivityManager.ActiveNetwork;

            if (activeNetwork == null)
            {
                return Task.FromResult<string?>(null);
            }

            var linkProperties =
                connectivityManager.GetLinkProperties(activeNetwork);

            if (linkProperties == null)
            {
                return Task.FromResult<string?>(null);
            }

            foreach (var linkAddress in linkProperties.LinkAddresses)
            {
                var address = linkAddress.Address;

                if (address == null)
                    continue;

                var ip = address.HostAddress;

                if (string.IsNullOrWhiteSpace(ip))
                    continue;

                if (ip.Contains(':'))
                    continue;

                return Task.FromResult<string?>(ip);
            }
            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            return Task.FromResult<string?>(null);
        }
    }

    private class ProvisioningNetworkCallback : ConnectivityManager.NetworkCallback
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
}
#endif