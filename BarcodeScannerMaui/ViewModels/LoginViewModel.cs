using System.Windows.Input;
using BarcodeScannerMaui.Services;

namespace BarcodeScannerMaui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string username;
        private string password;
        private string errorMessage;

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public delegate void LoginSuccessfulEventHandler();
        public event LoginSuccessfulEventHandler OnLoginSuccessful;

        public LoginViewModel()
        {
            Title = "Login";
            LoginCommand = new Command(OnLoginClicked);
        }

        private void OnLoginClicked()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password cannot be empty";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            bool isSuccess = AuthService.Login(Username, Password);

            IsBusy = false;

            if (isSuccess)
            {
                OnLoginSuccessful?.Invoke();
            }
            else
            {
                ErrorMessage = "Invalid credentials";
            }
        }
    }
}
