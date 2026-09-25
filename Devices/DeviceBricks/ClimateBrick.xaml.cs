using Models = Haven.Models;

namespace Haven.Devices.DeviceBricks
{
    public partial class ClimateBrick : ContentView
    {
        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(nameof(IconSource), typeof(string), typeof(ClimateBrick), "thermometer.png");

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ClimateBrick), string.Empty);

        public static readonly BindableProperty TemperatureProperty =
            BindableProperty.Create(nameof(Temperature), typeof(string), typeof(ClimateBrick), string.Empty);

        public static readonly BindableProperty HumidityProperty =
            BindableProperty.Create(nameof(Humidity), typeof(string), typeof(ClimateBrick), string.Empty);

        public static readonly BindableProperty DeviceProperty =
            BindableProperty.Create(nameof(Device), typeof(DeviceInfo), typeof(ClimateBrick), null, propertyChanged: OnDeviceChanged);

        public string IconSource
        {
            get => (string)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Temperature
        {
            get => (string)GetValue(TemperatureProperty);
            private set => SetValue(TemperatureProperty, value);
        }

        public string Humidity
        {
            get => (string)GetValue(HumidityProperty);
            private set => SetValue(HumidityProperty, value);
        }

        public Models.DeviceInfo? Device
        {
            get => (Models.DeviceInfo?)GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        public ClimateBrick() => InitializeComponent();

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var brick = (ClimateBrick)bindable;

            if (oldValue is Models.DeviceInfo old)
                old.StateChanged -= brick.OnDeviceStateChanged;

            if (newValue is Models.DeviceInfo updated)
                updated.StateChanged += brick.OnDeviceStateChanged;

            brick.Refresh();
        }

        private void OnDeviceStateChanged() => Refresh();

        private void Refresh()
        {
            Temperature = FormatOrDash(Device?.GetState("temperature"));
            Humidity = FormatOrDash(Device?.GetState("humidity"));
        }

        private static string FormatOrDash(string? raw) =>
            double.TryParse(raw, out var value) ? value.ToString("0") : "--";
    }
}