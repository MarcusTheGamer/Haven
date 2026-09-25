using Models = Haven.Models;

namespace Haven.Services;

public interface IDeviceRegistry
{
    void Register(Models.DeviceInfo device);
    Models.DeviceInfo? Get(string deviceId);
    IReadOnlyCollection<Models.DeviceInfo> All { get; }
    event Action? DevicesChanged;

    // refresh saved device ips
    Task RefreshStaleIpsAsync();
}