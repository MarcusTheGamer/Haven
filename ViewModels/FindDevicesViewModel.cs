using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Models;
using Haven.Services;
using System.Collections.ObjectModel;

namespace Haven.ViewModels;

public partial class FindDevicesViewModel : ObservableObject
{
    private readonly DeviceDiscoveryService _discovery;
    private readonly BTService _bt;

    private CancellationTokenSource? _searchCancellation;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool isProvisioning;

    public ObservableCollection<Models.DeviceInfo> Devices { get; } = new();

    public FindDevicesViewModel(DeviceDiscoveryService discovery, BTService bt)
    {
        _discovery = discovery;
        _bt = bt;
    }

    public async Task StartSearchingAsync()
    {
        if (IsSearching)
            return;

        IsSearching = true;
        _searchCancellation = new CancellationTokenSource();

        Devices.Clear();

        try
        {
            while (!_searchCancellation.Token.IsCancellationRequested)
            {
                var devices =
                    await _discovery.DiscoverDevicesAsync(
                        TimeSpan.FromSeconds(3));

                foreach (var device in devices)
                {
                    if (!Devices.Any(x => x.Id == device.Id))
                        Devices.Add(device);
                }

                await Task.Delay(
                    500,
                    _searchCancellation.Token);
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsSearching = false;
        }
    }

    public void StopSearching()
    {
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = null;
    }

    public async Task<bool> ProvisionDeviceAsync(Models.DeviceInfo device, string ssid, string password)
    {
        if (IsProvisioning)
            return false;

        IsProvisioning = true;

        try
        {
            return await _bt.ProvisionDeviceAsync(
                device.BleDevice,
                ssid,
                password);
        }
        finally
        {
            IsProvisioning = false;
        }
    }
}