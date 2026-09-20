using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models = Haven.Models;
using Services = Haven.Services;

namespace Haven.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly Services.SupabaseService _supabase;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(Services.SupabaseService supabase)
    {
        _supabase = supabase;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading)
            return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email or password missing.";
            return;
        }

        IsLoading = true;

        try
        {
            var login = new Models.LoginModel
            {
                Email = Email.Trim(),
                Password = Password
            };

            var success = await _supabase.LoginAsync(login);

            if (!success)
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while logging in.";
            Console.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.SignUpPage));
    }
}