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
        // remember the home network before connecting to the device
        var ssid = await _wifi.GetCurrentHomeSsidAsync();

        if (!string.IsNullOrEmpty(ssid))
            HomeSsid = ssid;
    }

    public async Task<bool> ProvisionAsync()
    {
        if (IsBusy) return false;

        if (string.IsNullOrWhiteSpace(HomeSsid))
        {
            StatusMessage = "Enter your Wi-Fi name and password.";
            return false;
        }

        IsBusy = true;

        try
        {
            StatusMessage = "Connecting to device...";

            // connect to the device's temporary Wi-Fi
            if (!await _wifi.ConnectToDeviceApAsync(ApSsid))
            {
                StatusMessage = "Couldn't connect to the device's Wi-Fi.";
                return false;
            }

            // read the device's real identity before provisioning
            StatusMessage = "Reading device info...";
            var info = await _wifi.ReadDeviceInfoAsync();

            // send the home Wi-Fi credentials to the device
            StatusMessage = "Sending your Wi-Fi details to the device...";
            var accepted = await _wifi.ProvisionDeviceAsync(HomeSsid, HomePassword);

            if (!accepted)
            {
                StatusMessage = "Device rejected the Wi-Fi details.";
                return false;
            }

            // wait while the device switches to the home network
            StatusMessage = "Waiting for the device to join your Wi-Fi...";
            var joinResult = await PollProvisionStatusAsync();

            // leave the device AP and return to the home network
            _wifi.DisconnectFromDeviceAp();

            if (joinResult == false)
            {
                StatusMessage = "The device couldn't join that Wi-Fi network. Double-check the password and try again.";
                return false;
            }

            var expectedId = info?.Id ?? DeviceId;

            // search the home network for the newly connected device
            StatusMessage = "Looking for the device on your network...";
            var foundIp = await _sweep.FindDeviceIpAsync(expectedId);

            // save the device and its new home-network address
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
                : "Device joined your Wi-Fi, but couldn't be found on your network yet — you can retry from device settings.";

            return true;
        }
        catch
        {
            StatusMessage = "Something went wrong adding the device.";
            return false;
        }
        finally
        {
            // make sure we leave the setup network
            _wifi.DisconnectFromDeviceAp();
            IsBusy = false;
        }
    }

    // wait for the device to finish connecting
    private async Task<bool?> PollProvisionStatusAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(18);
        var sawAnyResponse = false;

        while (DateTime.UtcNow < deadline)
        {
            var status = await _wifi.GetStatusAsync();

            if (status != null)
                sawAnyResponse = true;

            if (string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase))
                return false;

            // give the device time to connect
            await Task.Delay(1000);
        }

        // no response usually means the device already left its AP
        return sawAnyResponse ? false : null;
    }
}