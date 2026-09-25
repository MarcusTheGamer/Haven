using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models = Haven.Models;
using Services = Haven.Services;

namespace Haven.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly Services.IAuthService _supabase;
    private readonly Services.IPostAuthRouter _router;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public LoginViewModel(Services.IAuthService supabase, Services.IPostAuthRouter router)
    {
        _supabase = supabase;
        _router = router;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email or password missing.";
            return;
        }

        IsLoading = true;

        try
        {
            var login = new Models.LoginModel { Email = Email.Trim(), Password = Password };

            // authenticate the user
            if (!await _supabase.LoginAsync(login))
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            // route based on family membership
            await _router.RouteAsync();
        }
        catch
        {
            ErrorMessage = "An error occurred while logging in.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SignUpAsync() => await Shell.Current.GoToAsync(nameof(Views.SignUpPage));
}