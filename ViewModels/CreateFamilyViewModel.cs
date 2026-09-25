using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Haven.Services;

namespace Haven.ViewModels;

public partial class CreateFamilyViewModel : ObservableObject
{
    private readonly IFamilyService _families;
    private readonly ICurrentFamilyService _currentFamily;
    private readonly WiFiProvisioningService _wifi;

    [ObservableProperty] private string familyName = string.Empty;
    [ObservableProperty] private string detectedNetwork = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public CreateFamilyViewModel(IFamilyService families, ICurrentFamilyService currentFamily, WiFiProvisioningService wifi)
    {
        _families = families;
        _currentFamily = currentFamily;
        _wifi = wifi;
    }

    public async Task DetectNetworkAsync()
    {
        // detect the current home network
        var ssid = await _wifi.GetCurrentHomeSsidAsync();
        DetectedNetwork = string.IsNullOrEmpty(ssid) ? "Not detected — check Wi-Fi permissions" : ssid;
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FamilyName))
        {
            ErrorMessage = "Give your home a name.";
            return;
        }

        IsBusy = true;

        try
        {
            // create the family in supabase
            var family = await _families.CreateFamilyAsync(FamilyName.Trim());
            _currentFamily.CurrentFamily = family;

            // keep the network association locally
            if (!string.IsNullOrEmpty(DetectedNetwork))
                Preferences.Set($"family_{family.Id}_network", DetectedNetwork);

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch
        {
            ErrorMessage = "Couldn't create your family. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}