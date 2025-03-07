using BarcodeScannerMaui.Services;
using BarcodeScannerMaui.Views;

namespace BarcodeScannerMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (AuthService.IsLoggedIn)
                MainPage = new AppShell();
            else
                MainPage = new LoginPage();
        }
    }
}
