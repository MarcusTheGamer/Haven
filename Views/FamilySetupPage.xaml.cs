namespace Haven.Views;

public partial class FamilySetupPage : ContentPage
{
    public FamilySetupPage()
    {
        InitializeComponent();
    }

    private async void OnCreateClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(CreateFamilyPage));

    private async void OnJoinClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(InviteToFamilyPage));
}