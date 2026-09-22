using Haven.ViewModels;

namespace Haven.Views;

public partial class AddDevicePage : ContentPage
{
    private readonly AddDeviceViewModel _viewModel;

    public AddDevicePage(AddDeviceViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CaptureHomeNetworkAsync();
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        if (_viewModel.IsBusy)
            return;

        if (await _viewModel.ProvisionAsync())
            await Shell.Current.GoToAsync("..");
    }
}