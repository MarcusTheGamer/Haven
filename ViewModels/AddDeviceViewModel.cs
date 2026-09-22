using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Services;
using Plugin.BLE.Abstractions;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.ViewModels;

public partial class AddDeviceViewModel : ObservableObject, IQueryAttributable
{
    private readonly WiFiProvisioningService _wifi;
    private readonly IDeviceRegistry _registry;

    [ObservableProperty] private string apSsid = string.Empty;
    [ObservableProperty] private string deviceName = string.Empty;
    [ObservableProperty] private string deviceType = string.Empty;
    [ObservableProperty] private string deviceId = string.Empty;
    [ObservableProperty] private string homeSsid = string.Empty;
    [ObservableProperty] private string homePassword = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    public AddDeviceViewModel(WiFiProvisioningService wifi, IDeviceRegistry registry)
    {
        _wifi = wifi;
        _registry = registry;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ApSsid", out var v1)) ApSsid = v1?.ToString() ?? string.Empty;
        if (query.TryGetValue("DeviceType", out var v2)) DeviceType = v2?.ToString() ?? string.Empty;
        if (query.TryGetValue("DeviceId", out var v3)) DeviceId = v3?.ToString() ?? string.Empty;
        if (query.TryGetValue("DeviceName", out var v4)) DeviceName = v4?.ToString() ?? string.Empty;
    }

    public async Task CaptureHomeNetworkAsync()
    {
        var ssid = await _wifi.GetCurrentHomeSsidAsync();

        if (!string.IsNullOrEmpty(ssid))
            HomeSsid = ssid;
    }

    public async Task<bool> ProvisionAsync()
    {
        if (IsBusy)
            return false;

        if (string.IsNullOrWhiteSpace(HomeSsid) || string.IsNullOrWhiteSpace(HomePassword))
        {
            StatusMessage = "Enter your Wi-Fi name and password.";
            return false;
        }

        IsBusy = true;

        try
        {
            StatusMessage = "Connecting to device...";

            if (!await _wifi.ConnectToDeviceApAsync(ApSsid))
            {
                StatusMessage = "Couldn't connect to the device's Wi-Fi.";
                return false;
            }

            StatusMessage = "Reading device info...";
            var info = await _wifi.ReadDeviceInfoAsync();

            StatusMessage = "Sending your Wi-Fi details to the device...";
            if (!await _wifi.ProvisionDeviceAsync(HomeSsid, HomePassword))
            {
                StatusMessage = "Device rejected the Wi-Fi details.";
                return false;
            }

            StatusMessage = "Waiting for the device to join your network...";
            await Task.Delay(TimeSpan.FromSeconds(16));

            _registry.Register(new HavenDeviceInfo
            {
                Id = info?.Id ?? DeviceId,
                Type = info?.Type ?? DeviceType,
                Name = info?.Name ?? DeviceName
            });

            StatusMessage = "Device added.";
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AddDeviceViewModel] Provisioning failed: {ex}");
            StatusMessage = "Something went wrong adding the device.";
            return false;
        }
        finally
        {
            _wifi.DisconnectFromDeviceAp();
            IsBusy = false;
        }
    }
}