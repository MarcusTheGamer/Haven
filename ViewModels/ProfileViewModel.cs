using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services = Haven.Services;

namespace Haven.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly Services.IAuthService _auth;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string memberSince = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ProfileViewModel(Services.IAuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var profile = await _auth.GetCurrentUserAsync();

            if (profile == null)
            {
                ErrorMessage = "Could not load your profile.";
                return;
            }

            Email = profile.Email;

            FirstName = string.IsNullOrWhiteSpace(profile.FirstName)
                ? Email
                : profile.FirstName;

            MemberSince = profile.CreatedAt.HasValue
                ? $"Member since {profile.CreatedAt.Value:MMMM yyyy}"
                : string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Something went wrong loading your profile.";
            Console.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync($"//{nameof(Views.LoginPage)}");
    }
}