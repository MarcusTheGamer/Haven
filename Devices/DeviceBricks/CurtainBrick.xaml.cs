using Models = Haven.Models;
using Haven.Services;

namespace Haven.Devices.DeviceBricks
{
    public partial class CurtainBrick : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CurtainBrick), string.Empty);

        public static readonly BindableProperty DeviceProperty =
            BindableProperty.Create(nameof(Device), typeof(DeviceInfo), typeof(CurtainBrick), null, propertyChanged: OnDeviceChanged);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Models.DeviceInfo? Device
        {
            get => (Models.DeviceInfo?)GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        private IDeviceCommunicationService? _comms;

        public CurtainBrick() => InitializeComponent();

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var brick = (CurtainBrick)bindable;

            if (oldValue is Models.DeviceInfo old)
                old.StateChanged -= brick.OnDeviceStateChanged;

            if (newValue is Models.DeviceInfo updated)
                updated.StateChanged += brick.OnDeviceStateChanged;

            brick.Refresh();
        }

        private void OnDeviceStateChanged() => Refresh();

        private void Refresh()
        {
            var raw = Device?.GetState("position");
            PositionSlider.Value = double.TryParse(raw, out var value) ? value : 0;
        }

        private async void OnOpenClicked(object? sender, EventArgs e) => await SetPosition(100);
        private async void OnCloseClicked(object? sender, EventArgs e) => await SetPosition(0);
        private async void OnPositionCommitted(object? sender, EventArgs e) => await SetPosition(PositionSlider.Value);

        private async Task SetPosition(double position)
        {
            var device = Device;

            if (device is null || string.IsNullOrEmpty(device.Id))
                return;

            var previous = device.GetState("position");
            var desired = ((int)position).ToString();

            PositionSlider.Value = position;
            device.ApplyState(new[] { new KeyValuePair<string, string>("position", desired) });

            _comms ??= IPlatformApplication.Current?.Services.GetService<IDeviceCommunicationService>();

            var ok = _comms is not null &&
                     await _comms.SendAsync(device.Id, new Dictionary<string, string> { ["position"] = desired });

            if (!ok)
                device.ApplyState(new[] { new KeyValuePair<string, string>("position", previous ?? "0") });
        }
    }
}