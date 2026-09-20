using Haven.ViewModels;

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
}