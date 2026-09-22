namespace Haven.Devices.DeviceBricks;

[ContentProperty(nameof(BrickContent))]
public partial class BaseBrick : ContentView
{
    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(nameof(IconSource), typeof(string), typeof(BaseBrick), "sun.png");

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(BaseBrick), string.Empty);

    public static readonly BindableProperty BrickStyleProperty =
        BindableProperty.Create(
            nameof(BrickStyle),
            typeof(Style),
            typeof(BaseBrick),
            defaultValueCreator: _ =>
                Application.Current?.Resources.TryGetValue("BrickSingle", out var style) == true
                    ? style as Style
                    : null);

    public static readonly BindableProperty BrickContentProperty =
        BindableProperty.Create(nameof(BrickContent), typeof(View), typeof(BaseBrick), null);

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

    public Style BrickStyle
    {
        get => (Style)GetValue(BrickStyleProperty);
        set => SetValue(BrickStyleProperty, value);
    }

    public View BrickContent
    {
        get => (View)GetValue(BrickContentProperty);
        set => SetValue(BrickContentProperty, value);
    }

    public BaseBrick()
    {
        InitializeComponent();
    }
}