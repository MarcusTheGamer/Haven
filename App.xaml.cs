using Haven.Services;

namespace Haven;

public partial class App : Application
{
    private readonly IAuthService _authService;
    private readonly IPostAuthRouter _router;

    public App(
        IAuthService authService,
        IPostAuthRouter router)
    {
        InitializeComponent();

        _authService = authService;
        _router = router;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        _ = InitializeAsync();

        return window;
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _authService.InitializeAsync();

            if (!_authService.IsLoggedIn())
                return;

            await _router.RouteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[App] Initialization failed: {ex}");
        }
    }
}