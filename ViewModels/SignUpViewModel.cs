using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models = Haven.Models;
using Services = Haven.Services;

namespace Haven.ViewModels;

public partial class SignUpViewModel : ObservableObject
{
    private readonly Services.IAuthService _supabase;

    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public SignUpViewModel(Services.IAuthService supabase) => _supabase = supabase;

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (IsLoading) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "Please enter your first name.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email missing.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password missing";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsLoading = true;

        try
        {
            var signUp = new Models.SignUpModel
            {
                FirstName = FirstName.Trim(),
                Email = Email.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword
            };

            // create the account
            if (!await _supabase.SignUpAsync(signUp))
            {
                ErrorMessage = "An error occurred while creating your account.";
                return;
            }

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch
        {
            ErrorMessage = "An error occurred while creating your account.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync() => await Shell.Current.GoToAsync("//LoginPage");
}