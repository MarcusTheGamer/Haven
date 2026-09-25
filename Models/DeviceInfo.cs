namespace Haven.Models;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class DeviceInfo : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private string type = string.Empty;

    [ObservableProperty] private string ipAddress = string.Empty;
    [ObservableProperty] private string apSsid = string.Empty;

    private readonly Dictionary<string, string> state = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> State => state;

    public event Action? StateChanged;

    public void ApplyState(IEnumerable<KeyValuePair<string, string>> newState)
    {
        foreach (var kv in newState)
            state[kv.Key] = kv.Value;

        StateChanged?.Invoke();
    }

    public string? GetState(string key) =>
        state.TryGetValue(key, out var value) ? value : null;
}