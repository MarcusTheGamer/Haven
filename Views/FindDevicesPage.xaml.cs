using Haven.ViewModels;

namespace Haven.Views;

public partial class FindDevicesPage : ContentPage
{
	public FindDevicesPage()
	{
		InitializeComponent();

        BindingContext = new FindDevicesViewModel();
    }
}