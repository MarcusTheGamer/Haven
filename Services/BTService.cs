using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Text;

namespace Haven.Services;

public class BTService
{
    private readonly IAdapter _adapter;

    private static readonly Guid ServiceUuid =
        Guid.Parse("19B10000-E8F2-537E-4F6C-D104768A1214");

    private static readonly Guid DeviceInfoUuid =
        Guid.Parse("19B10001-E8F2-537E-4F6C-D104768A1214");

    private static readonly Guid SsidUuid =
        Guid.Parse("19B10002-E8F2-537E-4F6C-D104768A1214");

    private static readonly Guid PasswordUuid =
        Guid.Parse("19B10003-E8F2-537E-4F6C-D104768A1214");

    private static readonly Guid CommandUuid =
        Guid.Parse("19B10004-E8F2-537E-4F6C-D104768A1214");

    private static readonly Guid StatusUuid =
        Guid.Parse("19B10005-E8F2-537E-4F6C-D104768A1214");

    public BTService()
    {
        try
        {
            // init
            var bluetooth = CrossBluetoothLE.Current;

            _adapter = bluetooth.Adapter;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw;
        }
    }

    public async Task<List<IDevice>> DiscoverDevicesAsync(TimeSpan timeout)
    {
        var devices = new List<IDevice>();

        void DeviceDiscovered(object? sender, DeviceEventArgs args)
        {
            try
            {
                // adds to list of found devices if its not already in there
                if (args.Device == null)
                    return;

                if (devices.Any(x => x.Id == args.Device.Id))
                    return;

                devices.Add(args.Device);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        try
        {
            // subscribes to devices discovered by the bluetooth adapter
            _adapter.DeviceDiscovered += DeviceDiscovered;

            using var cts =
                new CancellationTokenSource(timeout);

            await _adapter.StartScanningForDevicesAsync(
                cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Scan timed out.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            // Cleanup to prevent memory leaks
            try
            {
                if (_adapter.IsScanning)
                {
                    await _adapter.StopScanningForDevicesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _adapter.DeviceDiscovered -= DeviceDiscovered;
        }

        return devices;
    }

    public async Task<string?> ReadDeviceInfoAsync(IDevice device)
    {
        try
        {
            if (device.State != DeviceState.Connected)
            {
                await _adapter.ConnectToDeviceAsync(device);
            }

            var service =
                await device.GetServiceAsync(ServiceUuid);

            if (service == null)
            {
                return null;
            }

            var characteristic =
                await service.GetCharacteristicAsync(
                    DeviceInfoUuid);

            if (characteristic == null)
            {
                return null;
            }

            var result =
                await characteristic.ReadAsync();

            var info =
                Encoding.UTF8.GetString(result.data);

            return info;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task<bool> ProvisionDeviceAsync(IDevice device, string ssid, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (device.State != DeviceState.Connected)
            {
                // tries to connect to it
                await _adapter.ConnectToDeviceAsync(
                    device,
                    cancellationToken: cancellationToken);
            }
            var service =
                await device.GetServiceAsync(ServiceUuid);

            if (service == null)
            {
                return false;
            }

            var ssidCharacteristic =
                await service.GetCharacteristicAsync(SsidUuid);

            var passwordCharacteristic =
                await service.GetCharacteristicAsync(PasswordUuid);

            var commandCharacteristic =
                await service.GetCharacteristicAsync(CommandUuid);

            var statusCharacteristic =
                await service.GetCharacteristicAsync(StatusUuid);

            if (ssidCharacteristic == null ||
                passwordCharacteristic == null ||
                commandCharacteristic == null ||
                statusCharacteristic == null)
            {
                return false;
            }

            // write wifi ssid and password to device so it can connect to the customers home network

            await ssidCharacteristic.WriteAsync(
                Encoding.UTF8.GetBytes(ssid));

            await passwordCharacteristic.WriteAsync(
                Encoding.UTF8.GetBytes(password));

            await commandCharacteristic.WriteAsync(
                Encoding.UTF8.GetBytes("PROVISION"));

            var deadline =
                DateTime.UtcNow.AddSeconds(20);

            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result =
                    await statusCharacteristic.ReadAsync();

                var status =
                    Encoding.UTF8.GetString(result.data);

                if (status == "SUCCESS")
                    return true;

                if (status == "FAILED")
                    return false;

                await Task.Delay(
                    500,
                    cancellationToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
        finally
        {
            try
            {
                if (device.State == DeviceState.Connected)
                {
                    await _adapter.DisconnectDeviceAsync(device);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}