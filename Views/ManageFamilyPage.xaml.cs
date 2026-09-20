using Haven.Models;

namespace Haven.Views;

public partial class ManageFamilyPage : ContentPage
{
    public ManageFamilyPage()
    {
        InitializeComponent();
    }

    private async void OnOptionTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border ||
            border.BindingContext is not RouteItem option)
            return;

        await Shell.Current.GoToAsync(option.Route);
    }
}