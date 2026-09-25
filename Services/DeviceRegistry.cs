using System.Collections.Concurrent;
using System.Text.Json;
using Models = Haven.Models;

namespace Haven.Services;

public class DeviceRegistry : IDeviceRegistry
{
    private static readonly string StorePath = Path.Combine(FileSystem.AppDataDirectory, "devices.json");
    private readonly ConcurrentDictionary<string, Models.DeviceInfo> _devices = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _fileLock = new();
    private readonly WiFiProvisioningService _wifi;

    public event Action? DevicesChanged;

    public DeviceRegistry(WiFiProvisioningService wifi)
    {
        _wifi = wifi;
        LoadFromDisk();
    }

    public void Register(Models.DeviceInfo device)
    {
        if (string.IsNullOrWhiteSpace(device.Id)) return;

        _devices[device.Id] = device;
        SaveToDisk();
        DevicesChanged?.Invoke();
    }

    public Models.DeviceInfo? Get(string deviceId) =>
        !string.IsNullOrEmpty(deviceId) && _devices.TryGetValue(deviceId, out var device) ? device : null;

    public IReadOnlyCollection<Models.DeviceInfo> All => _devices.Values.ToList();

    public async Task RefreshStaleIpsAsync()
    {
        foreach (var device in _devices.Values.ToList())
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(device.IpAddress))
                {
                    var existing = await _wifi.ReadDeviceInfoAsync(device.IpAddress);

                    if (existing != null && string.Equals(existing.Id, device.Id, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // refresh device ip
                var result = await _wifi.FindDeviceOnHomeNetworkAsync(device.Id);
                if (result == null) continue;

                if (!string.Equals(result.Ip, device.IpAddress, StringComparison.OrdinalIgnoreCase))
                {
                    device.IpAddress = result.Ip;
                    SaveToDisk();
                    DevicesChanged?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // refresh was cancelled
            }
            catch
            {
                // ignore refresh errors
            }
        }
    }

    // load saved devices
    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(StorePath)) return;

            var json = File.ReadAllText(StorePath);
            var saved = JsonSerializer.Deserialize<List<Models.DeviceInfo>>(json);
            if (saved == null) return;

            foreach (var device in saved)
            {
                if (!string.IsNullOrWhiteSpace(device.Id))
                    _devices[device.Id] = device;
            }
        }
        catch
        {
            // ignore load errors
        }
    }

    // save current devices
    private void SaveToDisk()
    {
        var snapshot = _devices.Values.ToList();

        lock (_fileLock)
        {
            try
            {
                var json = JsonSerializer.Serialize(snapshot);
                File.WriteAllText(StorePath, json);
            }
            catch
            {
                // ignore save errors
            }
        }
    }
}