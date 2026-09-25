using Haven.Models;
using Haven.ViewModels;

namespace Haven.Views;

public partial class ManageFamilyPage : ContentPage
{
    private readonly ManageFamilyViewModel _viewModel;

    public ManageFamilyPage(ManageFamilyViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void OnDotsClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Family family)
            OptionsMenu.BindingContext = family;

        OptionsMenu.IsVisible = !OptionsMenu.IsVisible;
    }

    private async void OnRenameClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Family family)
            return;

        OptionsMenu.IsVisible = false;

        var newName = await DisplayPromptAsync("Rename Family", "New name", initialValue: family.Name);

        if (!string.IsNullOrWhiteSpace(newName))
            await _viewModel.RenameAsync(family, newName.Trim());
    }

    private void OnManageMembersClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Family family)
            return;

        OptionsMenu.IsVisible = false;

        // TODO: Manage members
    }

    private async void OnInviteClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Family family)
            return;

        OptionsMenu.IsVisible = false;

        await Shell.Current.GoToAsync($"{nameof(FamilyQrCodePage)}?FamilyId={family.Id}");
    }

    private async void OnLeaveFamilyClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Family family)
            return;

        OptionsMenu.IsVisible = false;

        var confirmed = await DisplayAlert("Leave Family", $"Leave \"{family.Name}\"?", "Leave", "Cancel");

        if (confirmed)
            await _viewModel.LeaveAsync(family);
    }
}