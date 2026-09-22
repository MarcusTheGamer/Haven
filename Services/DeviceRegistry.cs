using System.Collections.Concurrent;
using Models = Haven.Models;

namespace Haven.Services;

public class DeviceRegistry : IDeviceRegistry
{
    private readonly ConcurrentDictionary<string, Models.DeviceInfo> _devices = new(StringComparer.OrdinalIgnoreCase);

    public event Action? DevicesChanged;

    public void Register(Models.DeviceInfo device)
    {
        if (string.IsNullOrEmpty(device.Id))
            return;

        _devices[device.Id] = device;
        DevicesChanged?.Invoke();
    }

    public Models.DeviceInfo? Get(string deviceId) =>
        !string.IsNullOrEmpty(deviceId) && _devices.TryGetValue(deviceId, out var device) ? device : null;

    public IReadOnlyCollection<Models.DeviceInfo> All => _devices.Values.ToList();
}