using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Services;
using System.Collections.ObjectModel;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.ViewModels;

public partial class FindDevicesViewModel : ObservableObject
{
    private readonly WiFiProvisioningService _wifi;
    private CancellationTokenSource? _searchCancellation;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool isProvisioning;

    public ObservableCollection<HavenDeviceInfo> Devices { get; } = new();

    public FindDevicesViewModel(WiFiProvisioningService wifi)
    {
        _wifi = wifi;
    }

    public async Task StartSearchingAsync()
    {
        if (IsSearching)
            return;

        IsSearching = true;

        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = new CancellationTokenSource();

        Devices.Clear();

        try
        {
            Console.WriteLine("[FindDevices] Starting discovery.");

            var networks = await _wifi.DiscoverDevicesAsync();

            foreach (var ssid in networks)
            {
                if (_searchCancellation.Token.IsCancellationRequested)
                    break;

                var device = CreateDeviceFromSsid(ssid);

                if (device == null)
                    continue;

                if (!Devices.Any(x =>
                    string.Equals(
                        x.Id,
                        device.Id,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    Devices.Add(device);

                    Console.WriteLine(
                        $"[FindDevices] Added {device.Name} ({device.Id}).");
                }
            }

            Console.WriteLine(
                $"[FindDevices] Discovery complete. {Devices.Count} device(s) found.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[FindDevices] Discovery cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FindDevices] Discovery failed: {ex}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private static HavenDeviceInfo? CreateDeviceFromSsid(string ssid)
    {
        if (!ssid.StartsWith(
                "HAVEN-",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var parts = ssid.Split('-');

        if (parts.Length < 3)
            return null;

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
        Console.WriteLine("[FindDevices] Stopping discovery.");

        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = null;

        IsSearching = false;
    }

    public async Task<bool> ConnectToDeviceAsync(string ssid)
    {
        return await _wifi.ConnectToDeviceApAsync(ssid);
    }

    public async Task<HavenDeviceInfo?> ReadConnectedDeviceInfoAsync(
        CancellationToken cancellationToken = default)
    {
        return await _wifi.ReadDeviceInfoAsync(
            WiFiProvisioningService.DefaultDeviceIp,
            cancellationToken);
    }

    public async Task<bool> ProvisionDeviceAsync(
        string ssid,
        string password)
    {
        if (IsProvisioning)
            return false;

        IsProvisioning = true;

        try
        {
            return await _wifi.ProvisionDeviceAsync(
                ssid,
                password);
        }
        finally
        {
            IsProvisioning = false;
        }
    }

    public async Task<string?> GetProvisioningStatusAsync()
    {
        return await _wifi.GetStatusAsync();
    }

    public void DisconnectFromDevice()
    {
        _wifi.DisconnectFromDeviceAp();
    }
}