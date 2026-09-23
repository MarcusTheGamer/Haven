using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Haven.Services;

/// <summary>
/// Finds a Haven device on the home LAN by sweeping the phone's current
/// /24 subnet and hitting GET /info on each candidate address, matching
/// on the device's unique Id (e.g. "SmartLamp-1a2b"). Used right after
/// provisioning, once the phone is back on the home network the device
/// was just told to join.
///
/// This exists because there's no reliable way to go from "the device's
/// MAC address" to "its current IP" after the fact — modern Android
/// blocks apps from reading the system ARP table, so resolving MAC to IP
/// isn't an option. Actively asking the network (this class) sidesteps
/// that entirely, and reuses the /info endpoint the device already has.
/// </summary>
public class DeviceNetworkSweepService
{
    private static readonly TimeSpan PerHostTimeout = TimeSpan.FromMilliseconds(400);
    private const int MaxConcurrentRequests = 32;

    /// <summary>
    /// Sweeps the phone's current /24 subnet looking for a device whose
    /// /info response has an Id matching expectedDeviceId. Returns the IP
    /// it found the device at, or null if nothing matched within
    /// overallTimeout (default 20s).
    /// </summary>
    public async Task<string?> FindDeviceIpAsync(
        string expectedDeviceId,
        TimeSpan? overallTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var localIp = GetLocalIPv4();

        if (localIp == null)
        {
            Console.WriteLine("[DeviceNetworkSweepService] Couldn't determine local IP address — is Wi-Fi connected?");
            return null;
        }

        var bytes = localIp.GetAddressBytes();
        var subnetPrefix = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.";
        var localIpString = localIp.ToString();

        Console.WriteLine($"[DeviceNetworkSweepService] Sweeping {subnetPrefix}0/24 for '{expectedDeviceId}'...");

        using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(overallTimeout ?? TimeSpan.FromSeconds(20));

        using var throttle = new SemaphoreSlim(MaxConcurrentRequests);

        string? foundIp = null;

        var tasks = new List<Task>();

        for (var host = 1; host <= 254; host++)
        {
            var candidateIp = $"{subnetPrefix}{host}";

            if (candidateIp == localIpString)
                continue;

            tasks.Add(CheckHostAsync(
                candidateIp,
                expectedDeviceId,
                throttle,
                overallCts,
                ip => foundIp = ip));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected once a match is found (cancels remaining in-flight
            // requests early) or the overall timeout elapses.
        }

        Console.WriteLine(foundIp != null
            ? $"[DeviceNetworkSweepService] Found '{expectedDeviceId}' at {foundIp}"
            : $"[DeviceNetworkSweepService] '{expectedDeviceId}' not found on {subnetPrefix}0/24");

        return foundIp;
    }

    private static async Task CheckHostAsync(
        string ip,
        string expectedDeviceId,
        SemaphoreSlim throttle,
        CancellationTokenSource overallCts,
        Action<string> onFound)
    {
        var acquired = false;

        try
        {
            await throttle.WaitAsync(overallCts.Token).ConfigureAwait(false);
            acquired = true;

            if (overallCts.IsCancellationRequested)
                return;

            using var http = new HttpClient
            {
                BaseAddress = new Uri($"http://{ip}"),
                Timeout = PerHostTimeout
            };

            using var perHostCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
            perHostCts.CancelAfter(PerHostTimeout);

            var info = await http.GetStringAsync("/info", perHostCts.Token).ConfigureAwait(false);

            var parts = info.Split(';');

            if (parts.Length >= 5 &&
                string.Equals(parts[0], "HAVEN_DEVICE", StringComparison.Ordinal) &&
                string.Equals(parts[2], expectedDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                onFound(ip);
                overallCts.Cancel(); // stop sweeping once we've found our match
            }
        }
        catch (OperationCanceledException)
        {
            // Per-host timeout, or another host already found the match — both expected.
        }
        catch (Exception)
        {
            // Connection refused / host down / not a Haven device — expected for
            // almost every address in a /24 sweep, not worth logging per-host.
        }
        finally
        {
            if (acquired)
                throttle.Release();
        }
    }

    private static IPAddress? GetLocalIPv4()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                continue;

            foreach (var addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(addr.Address))
                {
                    return addr.Address;
                }
            }
        }

        return null;
    }
}
