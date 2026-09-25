using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Haven.Services;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IDeviceRegistry _registry;
    private int _refreshStarted;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<HavenDeviceInfo> Devices { get; } = new();

    public MainPageViewModel(IDeviceRegistry registry)
    {
        _registry = registry;
        _registry.DevicesChanged += OnDevicesChanged;
    }

    private void OnDevicesChanged() => MainThread.BeginInvokeOnMainThread(Refresh);

    public void Refresh()
    {
        var latest = _registry.All.ToDictionary(d => d.Id, StringComparer.OrdinalIgnoreCase);

        // remove devices no longer in the registry
        for (var i = Devices.Count - 1; i >= 0; i--)
        {
            if (!latest.ContainsKey(Devices[i].Id))
                Devices.RemoveAt(i);
        }

        // add new devices and update changed ones
        foreach (var device in latest.Values)
        {
            var index = IndexOf(device.Id);

            if (index < 0)
                Devices.Add(device);
            else if (!ReferenceEquals(Devices[index], device))
                Devices[index] = device;
        }
    }

    public async Task RefreshDeviceIpsAsync()
    {
        if (Interlocked.Exchange(ref _refreshStarted, 1) != 0)
            return;

        try
        {
            // run network scanning away from the ui thread
            await Task.Run(async () =>
            {
                try
                {
                    await _registry.RefreshStaleIpsAsync();
                }
                catch
                {
                    // ignore refresh errors
                }
            });
        }
        finally
        {
            Interlocked.Exchange(ref _refreshStarted, 0);
        }
    }

    private int IndexOf(string deviceId)
    {
        for (var i = 0; i < Devices.Count; i++)
        {
            if (string.Equals(Devices[i].Id, deviceId, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }
}