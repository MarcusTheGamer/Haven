using Haven.ViewModels;

namespace Haven.Views;

public partial class CreateFamilyPage : ContentPage
{
    private readonly CreateFamilyViewModel _viewModel;

    public CreateFamilyPage(CreateFamilyViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.DetectNetworkAsync();
    }
}