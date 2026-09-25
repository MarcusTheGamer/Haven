using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Services;
using System.Collections.ObjectModel;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.ViewModels;

public partial class FindDevicesViewModel : ObservableObject
{
    private readonly WiFiProvisioningService _wifi;
    private CancellationTokenSource? _searchCancellation;

    [ObservableProperty] private bool isSearching;
    [ObservableProperty] private bool isProvisioning;

    public ObservableCollection<HavenDeviceInfo> Devices { get; } = new();

    public FindDevicesViewModel(WiFiProvisioningService wifi) => _wifi = wifi;

    public async Task StartSearchingAsync()
    {
        if (IsSearching) return;

        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();

        var cts = new CancellationTokenSource();
        _searchCancellation = cts;

        // keep a safe local token for this search
        var token = cts.Token;
        IsSearching = true;

        MainThread.BeginInvokeOnMainThread(Devices.Clear);

        try
        {
            var networks = await _wifi.DiscoverDevicesAsync();

            foreach (var ssid in networks)
            {
                if (token.IsCancellationRequested)
                    break;

                var device = CreateDeviceFromSsid(ssid);
                if (device == null) continue;

                // don't add the same device twice
                if (!Devices.Any(x => string.Equals(x.Id, device.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    var found = device;

                    // collections must be changed on the ui thread
                    MainThread.BeginInvokeOnMainThread(() => Devices.Add(found));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // search was cancelled
        }
        catch
        {
            // discovery failed
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsSearching = false;
        }
    }

    // convert a haven ssid into device info
    private static HavenDeviceInfo? CreateDeviceFromSsid(string ssid)
    {
        if (!ssid.StartsWith("HAVEN-", StringComparison.OrdinalIgnoreCase))
            return null;

        var parts = ssid.Split('-');
        if (parts.Length < 3) return null;

        var type = parts[1];
        var id = parts[2];

        return new HavenDeviceInfo
        {
            Id = id,
            Type = type,
            Name = type,
            ApSsid = ssid
        };
    }

    public void StopSearching()
    {
        // cancel the active search
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = null;

        IsSearching = false;
    }

    public async Task<bool> ConnectToDeviceAsync(string ssid) =>
        await _wifi.ConnectToDeviceApAsync(ssid);

    public async Task<HavenDeviceInfo?> ReadConnectedDeviceInfoAsync(CancellationToken cancellationToken = default) =>
        await _wifi.ReadDeviceInfoAsync(WiFiProvisioningService.DefaultDeviceIp, cancellationToken);

    public async Task<bool> ProvisionDeviceAsync(string ssid, string password)
    {
        if (IsProvisioning) return false;

        IsProvisioning = true;

        try
        {
            return await _wifi.ProvisionDeviceAsync(ssid, password);
        }
        finally
        {
            IsProvisioning = false;
        }
    }

    public async Task<string?> GetProvisioningStatusAsync() => await _wifi.GetStatusAsync();

    public void DisconnectFromDevice() => _wifi.DisconnectFromDeviceAp();
}