using Haven.ViewModels;

namespace Haven.Views;

public partial class FamilyQrCodePage : ContentPage
{
    private readonly FamilyQrCodeViewModel _viewModel;

    public FamilyQrCodePage(FamilyQrCodeViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (string.IsNullOrEmpty(_viewModel.Code))
            await _viewModel.GenerateCommand.ExecuteAsync(null);
    }
}