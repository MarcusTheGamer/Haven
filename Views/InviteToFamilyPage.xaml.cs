using Haven.ViewModels;
using ZXing.Net.Maui;

namespace Haven.Views;

public partial class InviteToFamilyPage : ContentPage
{
    private readonly InviteToFamilyViewModel _viewModel;
    private bool _handled;

    public InviteToFamilyPage(InviteToFamilyViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _handled = false;
        CameraView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        CameraView.IsDetecting = false;
        base.OnDisappearing();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_handled)
            return;

        var value = e.Results?.FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(value))
            return;

        _handled = true;

        // BarcodesDetected fires off the UI thread.
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            CameraView.IsDetecting = false;

            var success = await _viewModel.RedeemAsync(value);

            if (success)
            {
                await Task.Delay(600);
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Task.Delay(2000);
                _handled = false;
                CameraView.IsDetecting = true;
            }
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(FamilySetupPage));
}