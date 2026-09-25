using Models = Haven.Models;
using Haven.Services;

namespace Haven.Devices.DeviceBricks
{
    public partial class ToggleBrick : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ToggleBrick), string.Empty);

        public static readonly BindableProperty DeviceProperty =
            BindableProperty.Create(nameof(Device), typeof(DeviceInfo), typeof(ToggleBrick), null, propertyChanged: OnDeviceChanged);

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

        public ToggleBrick()
        {
            InitializeComponent();

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            Host.GestureRecognizers.Add(tap);
        }

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var brick = (ToggleBrick)bindable;

            if (oldValue is Models.DeviceInfo old)
                old.StateChanged -= brick.OnDeviceStateChanged;

            if (newValue is Models.DeviceInfo updated)
                updated.StateChanged += brick.OnDeviceStateChanged;

            brick.Refresh();
        }

        private void OnDeviceStateChanged() => Refresh();

        private bool IsOn => Device?.GetState("on") == "1";

        private void Refresh()
        {
            Host.IconSource = IsOn ? "sun.png" : "moon.png";
            StatusLabel.Text = IsOn ? "On" : "Off";
        }

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            var device = Device;

            if (device is null || string.IsNullOrEmpty(device.Id))
                return;

            var previous = device.GetState("on");
            var desired = !IsOn ? "1" : "0";

            device.ApplyState(new[] { new KeyValuePair<string, string>("on", desired) });

            _comms ??= IPlatformApplication.Current?.Services.GetService<IDeviceCommunicationService>();

            var ok = _comms is not null &&
                     await _comms.SendAsync(device.Id, new Dictionary<string, string> { ["on"] = desired });

            if (!ok)
                device.ApplyState(new[] { new KeyValuePair<string, string>("on", previous ?? "0") });
        }
    }
}