using CommunityToolkit.Mvvm.ComponentModel;
using Haven.Services;

namespace Haven.ViewModels;

public partial class InviteToFamilyViewModel : ObservableObject
{
    private readonly IFamilyService _families;
    private readonly ICurrentFamilyService _currentFamily;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Point your camera at the invite QR code";

    public InviteToFamilyViewModel(IFamilyService families, ICurrentFamilyService currentFamily)
    {
        _families = families;
        _currentFamily = currentFamily;
    }

    public async Task<bool> RedeemAsync(string code)
    {
        if (IsBusy) return false;

        IsBusy = true;
        StatusMessage = "Checking invite code...";

        try
        {
            // redeem the scanned pairing code
            var family = await _families.RedeemPairingCodeAsync(code);

            if (family == null)
            {
                StatusMessage = "That code is invalid or has expired.";
                return false;
            }

            // make the joined family active
            _currentFamily.CurrentFamily = family;
            StatusMessage = $"Joined {family.Name}!";
            return true;
        }
        catch
        {
            StatusMessage = "Something went wrong redeeming that code.";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}