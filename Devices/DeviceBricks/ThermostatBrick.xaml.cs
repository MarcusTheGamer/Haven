namespace Haven.Devices.DeviceBricks
{
    public partial class ThermostatBrick : ContentView
    {
        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(nameof(IconSource), typeof(string), typeof(ThermostatBrick), "moon.png");

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ThermostatBrick), string.Empty);

        public static readonly BindableProperty GaugeValueProperty =
            BindableProperty.Create(nameof(GaugeValue), typeof(double), typeof(ThermostatBrick), 0d);

        public static readonly BindableProperty GaugeMaxProperty =
            BindableProperty.Create(nameof(GaugeMax), typeof(double), typeof(ThermostatBrick), 40d);

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

        public double GaugeValue
        {
            get => (double)GetValue(GaugeValueProperty);
            set => SetValue(GaugeValueProperty, value);
        }

        public double GaugeMax
        {
            get => (double)GetValue(GaugeMaxProperty);
            set => SetValue(GaugeMaxProperty, value);
        }

        public ThermostatBrick()
        {
            InitializeComponent();
        }
    }
}