using Models = Haven.Models;

namespace Haven.Devices.DeviceBricks
{
    public partial class ThermostatBrick : ContentView
    {
        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(nameof(IconSource), typeof(string), typeof(ThermostatBrick), "thermometer.png");

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ThermostatBrick), string.Empty);

        public static readonly BindableProperty GaugeMaxProperty =
            BindableProperty.Create(nameof(GaugeMax), typeof(double), typeof(ThermostatBrick), 40d);

        public static readonly BindableProperty GaugeValueProperty =
            BindableProperty.Create(nameof(GaugeValue), typeof(double), typeof(ThermostatBrick), 0d);

        public static readonly BindableProperty DeviceProperty =
            BindableProperty.Create(nameof(Device), typeof(DeviceInfo), typeof(ThermostatBrick), null, propertyChanged: OnDeviceChanged);

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

        public double GaugeMax
        {
            get => (double)GetValue(GaugeMaxProperty);
            set => SetValue(GaugeMaxProperty, value);
        }

        public double GaugeValue
        {
            get => (double)GetValue(GaugeValueProperty);
            private set => SetValue(GaugeValueProperty, value);
        }

        public Models.DeviceInfo? Device
        {
            get => (Models.DeviceInfo?)GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        public ThermostatBrick() => InitializeComponent();

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var brick = (ThermostatBrick)bindable;

            if (oldValue is Models.DeviceInfo old)
                old.StateChanged -= brick.OnDeviceStateChanged;

            if (newValue is Models.DeviceInfo updated)
                updated.StateChanged += brick.OnDeviceStateChanged;

            brick.Refresh();
        }

        private void OnDeviceStateChanged() => Refresh();

        private void Refresh()
        {
            var raw = Device?.GetState("target_temperature");
            GaugeValue = double.TryParse(raw, out var value) ? value : 0;
        }
    }
}