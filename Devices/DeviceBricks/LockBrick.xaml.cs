namespace Haven.Devices.DeviceBricks
{
    public partial class LockBrick : ContentView
    {
        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(nameof(IconSource), typeof(string), typeof(LockBrick), "unlock.png");

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(LockBrick), string.Empty);

        public static readonly BindableProperty StatusTextProperty =
            BindableProperty.Create(nameof(StatusText), typeof(string), typeof(LockBrick), string.Empty);

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

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public LockBrick()
        {
            InitializeComponent();
        }
    }
}