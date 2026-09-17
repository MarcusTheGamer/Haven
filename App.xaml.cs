namespace Haven;

public partial class App : Application
{
    private readonly Services.SupabaseService _supabase;

    public App(Services.SupabaseService supabase)
    {
        InitializeComponent();

        _supabase = supabase;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        _ = InitializeAsync();

        return window;
    }

    private async Task InitializeAsync()
    {
        await _supabase.InitializeAsync();

        if (_supabase.IsLoggedIn())
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}