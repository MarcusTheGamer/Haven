using Haven.Models;
using Supabase;
using Supabase.Gotrue;

namespace Haven.Services;

public class SupabaseService : IAuthService
{
    private readonly Supabase.Client _client;

    public SupabaseService()
    {
        var url = "https://ihfhnvrdpwxqwfgdoirq.supabase.co";
        var key = "sb_publishable_mchLuPAPs1aJIhq9kwpFvw__mAWUrnQ";

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            SessionHandler = new SupabaseSessionHandler()
        };

        _client = new Supabase.Client(url, key, options);
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    public async Task<bool> LoginAsync(LoginModel login)
    {
        try
        {
            var session = await _client.Auth.SignIn(
                login.Email,
                login.Password);

            return session != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SignUpAsync(SignUpModel signUp)
    {
        try
        {
            var session = await _client.Auth.SignUp(
                signUp.Email,
                signUp.Password,
                new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                    { "first_name", signUp.FirstName }
                    }
                });

            return session != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sign up failed: {ex.Message}");
            return false;
        }
    }

    public bool IsLoggedIn()
    {
        return _client.Auth.CurrentSession != null;
    }

    public async Task LogoutAsync()
    {
        await _client.Auth.SignOut();
    }

    public Task<UserProfile?> GetCurrentUserAsync()
    {
        var user = _client.Auth.CurrentUser;

        if (user == null)
            return Task.FromResult<UserProfile?>(null);

        var firstName = string.Empty;

        if (user.UserMetadata != null &&
            user.UserMetadata.TryGetValue("first_name", out var value) &&
            value != null)
        {
            firstName = value.ToString() ?? string.Empty;
        }

        var profile = new UserProfile
        {
            Id = user.Id ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = firstName,
            CreatedAt = user.CreatedAt
        };

        return Task.FromResult<UserProfile?>(profile);
    }
}