using Models = Haven.Models;
using Haven.Services;

namespace Haven.Devices.DeviceBricks
{
    public partial class LampBrick : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(LampBrick), string.Empty);

        public static readonly BindableProperty DeviceProperty =
            BindableProperty.Create(nameof(Device), typeof(DeviceInfo), typeof(LampBrick), null, propertyChanged: OnDeviceChanged);

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
        private bool _suppressToggle;

        public LampBrick() => InitializeComponent();

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var brick = (LampBrick)bindable;

            if (oldValue is Models.DeviceInfo old)
                old.StateChanged -= brick.OnDeviceStateChanged;

            if (newValue is Models.DeviceInfo updated)
                updated.StateChanged += brick.OnDeviceStateChanged;

            brick.Refresh();
        }

        private void OnDeviceStateChanged() => Refresh();

        private void Refresh()
        {
            var device = Device;

            if (device is null)
                return;

            _suppressToggle = true;
            PowerSwitch.IsToggled = device.GetState("on") == "1";
            _suppressToggle = false;

            RedSlider.Value = ParseOr(device.GetState("r"), 0);
            GreenSlider.Value = ParseOr(device.GetState("g"), 0);
            BlueSlider.Value = ParseOr(device.GetState("b"), 0);
            UpdateSwatch();
        }

        private static double ParseOr(string? raw, double fallback) =>
            double.TryParse(raw, out var value) ? value : fallback;

        private void UpdateSwatch() =>
            Swatch.Color = Color.FromRgb((int)RedSlider.Value, (int)GreenSlider.Value, (int)BlueSlider.Value);

        private async void OnPowerToggled(object? sender, ToggledEventArgs e)
        {
            if (_suppressToggle)
                return;

            var device = Device;

            if (device is null)
                return;

            var previous = device.GetState("on");
            var desired = e.Value ? "1" : "0";

            device.ApplyState(new[] { new KeyValuePair<string, string>("on", desired) });

            if (!await SendAsync(new Dictionary<string, string> { ["on"] = desired }))
                device.ApplyState(new[] { new KeyValuePair<string, string>("on", previous ?? "0") });
        }

        private void OnColorPreview(object? sender, ValueChangedEventArgs e) => UpdateSwatch();

        // Sends once the drag ends, not on every intermediate tick, so
        // color changes don't flood the lamp mid-drag.
        private async void OnColorCommitted(object? sender, EventArgs e)
        {
            var device = Device;

            if (device is null)
                return;

            var previousR = device.GetState("r");
            var previousG = device.GetState("g");
            var previousB = device.GetState("b");

            var r = ((int)RedSlider.Value).ToString();
            var g = ((int)GreenSlider.Value).ToString();
            var b = ((int)BlueSlider.Value).ToString();

            device.ApplyState(new[]
            {
                new KeyValuePair<string, string>("r", r),
                new KeyValuePair<string, string>("g", g),
                new KeyValuePair<string, string>("b", b)
            });

            var ok = await SendAsync(new Dictionary<string, string> { ["r"] = r, ["g"] = g, ["b"] = b });

            if (!ok)
            {
                device.ApplyState(new[]
                {
                    new KeyValuePair<string, string>("r", previousR ?? "0"),
                    new KeyValuePair<string, string>("g", previousG ?? "0"),
                    new KeyValuePair<string, string>("b", previousB ?? "0")
                });
            }
        }

        private async Task<bool> SendAsync(Dictionary<string, string> payload)
        {
            var device = Device;

            if (device is null || string.IsNullOrEmpty(device.Id))
                return false;

            _comms ??= IPlatformApplication.Current?.Services.GetService<IDeviceCommunicationService>();

            return _comms is not null && await _comms.SendAsync(device.Id, payload);
        }
    }
}