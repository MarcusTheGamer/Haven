using Models = Haven.Models;

namespace Haven.Services;

public class DeviceDiscoveryService
{
    private readonly BTService _bt;

    public DeviceDiscoveryService(BTService bt)
    {
        _bt = bt;
    }

    public async Task<List<Models.DeviceInfo>> DiscoverDevicesAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        var bleDevices =
            await _bt.DiscoverDevicesAsync(timeout.Value);

        var devices = new List<Models.DeviceInfo>();

        foreach (var bleDevice in bleDevices)
        {
            var info =
                await _bt.ReadDeviceInfoAsync(bleDevice);

            if (string.IsNullOrWhiteSpace(info))
                continue;

            var parts =
                info.Split(';');

            if (parts.Length < 5)
                continue;

            if (parts[0] != "HAVEN_DEVICE")
                continue;

            if (devices.Any(x => x.Id == parts[2]))
                continue;

            devices.Add(new Models.DeviceInfo
            {
                BleDevice = bleDevice,
                Id = parts[2],
                Type = parts[3],
                Name = parts[4]
            });
        }

        return devices;
    }
}