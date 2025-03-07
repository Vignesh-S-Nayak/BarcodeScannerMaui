using BarcodeScannerMaui.ViewModels;

namespace BarcodeScannerMaui.Views
{
    public partial class LoginPage : ContentPage
    {
        private LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as LoginViewModel;
            _viewModel.OnLoginSuccessful += OnLoginSuccessful;
        }

        private void OnLoginSuccessful()
        {
            MainThread.BeginInvokeOnMainThread(() => {
                Application.Current.MainPage = new AppShell();
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnLoginSuccessful -= OnLoginSuccessful;
        }
    }
}
