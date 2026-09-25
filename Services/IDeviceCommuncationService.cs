namespace Haven.Services;

public interface IDeviceCommunicationService
{
    Task<bool> SendAsync(string deviceId, IReadOnlyDictionary<string, string> payload, CancellationToken cancellationToken = default);
}