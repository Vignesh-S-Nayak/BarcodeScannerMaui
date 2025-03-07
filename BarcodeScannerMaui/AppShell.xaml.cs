namespace BarcodeScannerMaui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("scan", typeof(Views.ScanPage));
            Routing.RegisterRoute("items", typeof(Views.ItemsPage));
        }
    }
}
