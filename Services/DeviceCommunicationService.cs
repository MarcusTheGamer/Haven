namespace Haven.Services;

public class DeviceCommunicationService : IDeviceCommunicationService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

    private readonly IDeviceRegistry _registry;

    public DeviceCommunicationService(IDeviceRegistry registry)
    {
        _registry = registry;
    }

    public async Task<bool> SendCommandAsync(string deviceId, string command, CancellationToken cancellationToken = default)
    {
        var device = _registry.Get(deviceId);

        if (device is null || string.IsNullOrEmpty(device.IpAddress))
        {
            Console.WriteLine($"[DeviceCommunicationService] No known IP for device '{deviceId}'.");
            return false;
        }

        try
        {
            Console.WriteLine($"[DeviceCommunicationService] POST http://{device.IpAddress}/command ({command})");

            using var http = CreateClient(device.IpAddress);

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("command", command)
            });

            var response = await http.PostAsync("/command", form, cancellationToken);

            Console.WriteLine($"[DeviceCommunicationService] /command returned {(int)response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeviceCommunicationService] SendCommandAsync failed: {ex}");
            return false;
        }
    }

    private static HttpClient CreateClient(string ipAddress) =>
        new HttpClient
        {
            BaseAddress = new Uri($"http://{ipAddress}"),
            Timeout = RequestTimeout
        };
}