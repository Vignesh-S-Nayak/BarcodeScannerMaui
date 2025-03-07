using BarcodeScannerMaui.ViewModels;

namespace BarcodeScannerMaui.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearing();
        }

        private async void OnRemoveItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string barcode = button.CommandParameter as string;
                if (!string.IsNullOrEmpty(barcode))
                {
                    await _viewModel.RemoveItem(barcode);
                }
            }
        }
    }
}
