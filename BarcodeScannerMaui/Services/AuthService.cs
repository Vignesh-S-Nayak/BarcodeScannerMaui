namespace BarcodeScannerMaui.Services
{
    public static class AuthService
    {
        public static bool IsLoggedIn { get; private set; } = false;

        public static bool Login(string username, string password)
        {
            if (username == "admin" && password == "password")
            {
                IsLoggedIn = true;
                return true;
            }
            return false;
        }

        public static void Logout()
        {
            IsLoggedIn = false;
        }
    }
}
