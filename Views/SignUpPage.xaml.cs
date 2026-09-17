using Haven.ViewModels;

namespace Haven.Views;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}