using Haven.Services;

namespace Haven;

public partial class App : Application
{
    private readonly IAuthService _authService;

public App(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        _ = InitializeAsync();

        return window;
    }

    private async Task InitializeAsync()
    {
        await _authService.InitializeAsync();

        if (_authService.IsLoggedIn())
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
