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
            set => SetValue(TemperatureProperty, value);
        }

        public string Humidity
        {
            get => (string)GetValue(HumidityProperty);
            set => SetValue(HumidityProperty, value);
        }

        public ClimateBrick()
        {
            InitializeComponent();
        }
    }
}