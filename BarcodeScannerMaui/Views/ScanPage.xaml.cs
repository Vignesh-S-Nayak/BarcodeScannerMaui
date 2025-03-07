using BarcodeScannerMaui.Services;

namespace BarcodeScannerMaui.Views
{
    public partial class ScanPage : ContentPage
    {
        public ScanPage()
        {
            InitializeComponent();
        }

        private async void OnScanButtonClicked(object sender, EventArgs e)
        {
            // Open the camera page as a modal
            await Navigation.PushModalAsync(new CameraPage());
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            AuthService.Logout();
            MainThread.BeginInvokeOnMainThread(() => {
                Application.Current.MainPage = new LoginPage();
            });
        }
    }
}
