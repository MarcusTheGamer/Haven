namespace Haven.Services;

public interface IDeviceCommunicationService
{
    Task<bool> SendCommandAsync(string deviceId, string command, CancellationToken cancellationToken = default);
}