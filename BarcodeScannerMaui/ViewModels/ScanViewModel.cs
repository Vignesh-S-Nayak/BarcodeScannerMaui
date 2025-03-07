using System.Windows.Input;
using BarcodeScannerMaui.Services;

namespace BarcodeScannerMaui.ViewModels
{
    public class ScanViewModel : BaseViewModel
    {
        private bool isScanning;
        private bool isProcessing;

        public bool IsScanning
        {
            get => isScanning;
            set => SetProperty(ref isScanning, value);
        }

        public ICommand ToggleScanCommand { get; }

        public delegate void ScanToggleEventHandler(bool startScanning);
        public event ScanToggleEventHandler OnScanToggle;

        public delegate void NavigateToItemsEventHandler();
        public event NavigateToItemsEventHandler OnNavigateToItems;

        public ScanViewModel()
        {
            Title = "Scan";
            ToggleScanCommand = new Command(ToggleScan);
            isProcessing = false;
        }

        public void ToggleScan()
        {
            IsScanning = !IsScanning;
            OnScanToggle?.Invoke(IsScanning);
        }

        public async Task ProcessScannedBarcode(string barcode)
        {
            if (isProcessing || string.IsNullOrWhiteSpace(barcode)) return;

            isProcessing = true;

            // Add or update the scanned item
            await ItemService.AddOrUpdateItem(barcode);

            // Navigate to the items page
            OnNavigateToItems?.Invoke();

            isProcessing = false;
        }
    }
}
