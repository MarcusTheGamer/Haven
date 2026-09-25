namespace Haven.Services;

public class DeviceCommunicationService : IDeviceCommunicationService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
    private readonly IDeviceRegistry _registry;

    public DeviceCommunicationService(IDeviceRegistry registry) => _registry = registry;

    public async Task<bool> SendAsync(string deviceId, IReadOnlyDictionary<string, string> payload, CancellationToken cancellationToken = default)
    {
        var device = _registry.Get(deviceId);
        if (device is null || string.IsNullOrEmpty(device.IpAddress)) return false;

        try
        {
            // send command
            using var http = CreateClient(device.IpAddress);
            var form = new FormUrlEncodedContent(payload);
            var response = await http.PostAsync("/command", form, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // create device client
    private static HttpClient CreateClient(string ipAddress) => new()
    {
        BaseAddress = new Uri($"http://{ipAddress}"),
        Timeout = RequestTimeout
    };
}