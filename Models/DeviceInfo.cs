using Plugin.BLE.Abstractions.Contracts;

namespace Haven.Models;

public class DeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string ApSsid { get; set; } = string.Empty;
}