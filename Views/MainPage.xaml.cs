using Models = Haven.Models;

namespace Haven.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }

        private async void Brick_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(DeviceDetailsPage));
        }
    }
}