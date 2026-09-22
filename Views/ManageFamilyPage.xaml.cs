namespace Haven.Views;

public partial class ManageFamilyPage : ContentPage
{
    public ManageFamilyPage()
    {
        InitializeComponent();
    }

    private void OnDotsClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button &&
            button.BindingContext is Haven.Models.RouteItem option)
        {
            OptionsMenu.BindingContext = option;
        }

        OptionsMenu.IsVisible = !OptionsMenu.IsVisible;
    }

    private void OnRenameClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Haven.Models.RouteItem family)
            return;

        OptionsMenu.IsVisible = false;

        // TODO: Rename family
    }

    private void OnManageMembersClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Haven.Models.RouteItem family)
            return;

        OptionsMenu.IsVisible = false;

        // TODO: Manage members
    }

    private void OnLeaveFamilyClicked(object sender, EventArgs e)
    {
        if (OptionsMenu.BindingContext is not Haven.Models.RouteItem family)
            return;

        OptionsMenu.IsVisible = false;

        // TODO: Leave family
    }
}