using Plugin.BLE.Abstractions.Contracts;

namespace Haven.Models;

public class DeviceInfo
{
    public IDevice BleDevice { get; set; } = null!;
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}