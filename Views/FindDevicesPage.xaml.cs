using Haven.ViewModels;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.Views;

public partial class FindDevicesPage : ContentPage
{
    private readonly FindDevicesViewModel _viewModel;

    public FindDevicesPage(FindDevicesViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.StartSearchingAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopSearching();
        base.OnDisappearing();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not HavenDeviceInfo device)
            return;

        _viewModel.StopSearching();

        await Shell.Current.GoToAsync(nameof(AddDevicePage), new Dictionary<string, object>
        {
            ["ApSsid"] = device.ApSsid,
            ["DeviceType"] = device.Type,
            ["DeviceId"] = device.Id,
            ["DeviceName"] = device.Name
        });
    }
}