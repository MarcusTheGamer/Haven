using Haven.Services;

namespace Haven.Devices.DeviceBricks
{
    public partial class ToggleBrick : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ToggleBrick), string.Empty);

        public static readonly BindableProperty StatusTextProperty =
            BindableProperty.Create(nameof(StatusText), typeof(string), typeof(ToggleBrick), string.Empty);

        public static readonly BindableProperty DeviceIdProperty =
            BindableProperty.Create(nameof(DeviceId), typeof(string), typeof(ToggleBrick), string.Empty);

        public static readonly BindableProperty IsOnProperty =
            BindableProperty.Create(nameof(IsOn), typeof(bool), typeof(ToggleBrick), false, propertyChanged: OnIsOnChanged);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public string DeviceId
        {
            get => (string)GetValue(DeviceIdProperty);
            set => SetValue(DeviceIdProperty, value);
        }

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        private IDeviceCommunicationService? _comms;

        public ToggleBrick()
        {
            InitializeComponent();

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            Host.GestureRecognizers.Add(tap);

            UpdateIcon();
        }

        private static void OnIsOnChanged(BindableObject bindable, object oldValue, object newValue) =>
            ((ToggleBrick)bindable).UpdateIcon();

        private void UpdateIcon() => Host.IconSource = IsOn ? "sun.png" : "moon.png";

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            var previous = IsOn;
            IsOn = !IsOn;

            if (string.IsNullOrEmpty(DeviceId))
                return;

            _comms ??= IPlatformApplication.Current?.Services.GetService<IDeviceCommunicationService>();

            var ok = _comms is not null &&
                     await _comms.SendCommandAsync(DeviceId, IsOn ? "on" : "off");

            if (!ok)
                IsOn = previous;
        }
    }
}