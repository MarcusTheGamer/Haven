using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Haven.Services;
using QRCoder;

namespace Haven.ViewModels;

public partial class FamilyQrCodeViewModel : ObservableObject, IQueryAttributable
{
    private readonly IFamilyService _families;

    [ObservableProperty] private long familyId;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private ImageSource? qrImageSource;
    [ObservableProperty] private string expiresText = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public FamilyQrCodeViewModel(IFamilyService families) => _families = families;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("FamilyId", out var value) &&
            long.TryParse(value?.ToString(), out var id))
        {
            FamilyId = id;
        }
    }

    [RelayCommand]
    private async Task GenerateAsync()
    {
        if (IsBusy || FamilyId == 0) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // create a short-lived pairing code
            var validFor = TimeSpan.FromMinutes(10);
            Code = await _families.GeneratePairingCodeAsync(FamilyId, validFor);
            ExpiresText = $"Expires {DateTime.Now.Add(validFor):t}";

            // turn the code into a qr image
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(Code, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            var bytes = png.GetGraphic(20);

            QrImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch
        {
            ErrorMessage = "Couldn't generate an invite code.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}