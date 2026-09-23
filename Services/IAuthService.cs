using Haven.Models;

namespace Haven.Services;

/// <summary>
/// Abstraction over the app's auth backend. Depend on this — not on
/// SupabaseService directly — from ViewModels/Pages, so swapping backends
/// later is a DI registration change, not a rewrite.
/// </summary>
public interface IAuthService
{
    Task InitializeAsync();
    Task<bool> LoginAsync(LoginModel login);
    Task<bool> SignUpAsync(SignUpModel signUp);
    bool IsLoggedIn();
    Task LogoutAsync();
    Task<UserProfile?> GetCurrentUserAsync();
}