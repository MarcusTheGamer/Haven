using Haven.Models;

namespace Haven.Services;

public interface IAuthService
{
    Task InitializeAsync();
    Task<bool> LoginAsync(LoginModel login);
    Task<bool> SignUpAsync(SignUpModel signUp);
    bool IsLoggedIn();
    Task LogoutAsync();
    Task<UserProfile?> GetCurrentUserAsync();
}