using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Services;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.ViewModels;

public partial class AddDeviceViewModel : ObservableObject, IQueryAttributable
{
    private readonly WiFiProvisioningService _wifi;
    private readonly IDeviceRegistry _registry;
    private readonly DeviceNetworkSweepService _sweep;

    [ObservableProperty] private string apSsid = string.Empty;
    [ObservableProperty] private string deviceName = string.Empty;
    [ObservableProperty] private string deviceType = string.Empty;
    [ObservableProperty] private string deviceId = string.Empty;
    [ObservableProperty] private string homeSsid = string.Empty;
    [ObservableProperty] private string homePassword = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    public AddDeviceViewModel(WiFiProvisioningService wifi, IDeviceRegistry registry, DeviceNetworkSweepService sweep)
    {
        _wifi = wifi;
        _registry = registry;
        _sweep = sweep;
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

        if (string.IsNullOrWhiteSpace(HomeSsid))
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
            var provisioned = await _wifi.ProvisionDeviceAsync(HomeSsid, HomePassword);

            // Either way, we're done talking to the device's own setup AP —
            // hop back to the home network now, since the sweep below has
            // to run FROM the home network to find the device on it.
            _wifi.DisconnectFromDeviceAp();

            if (!provisioned)
            {
                StatusMessage = "Device rejected the Wi-Fi details.";
                return false;
            }

            var expectedId = info?.Id ?? DeviceId;

            StatusMessage = "Waiting for the device to join your network...";
            var foundIp = await _sweep.FindDeviceIpAsync(expectedId);

            _registry.Register(new HavenDeviceInfo
            {
                Id = expectedId,
                Type = info?.Type ?? DeviceType,
                Name = info?.Name ?? DeviceName,
                IpAddress = foundIp ?? string.Empty,
                ApSsid = ApSsid
            });

            StatusMessage = foundIp != null
                ? "Device added."
                : "Device added, but couldn't be found on your network yet — you can retry from device settings.";

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