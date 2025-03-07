using ZXing.Net.Maui;
using BarcodeScannerMaui.Services;

namespace BarcodeScannerMaui.Views
{
    public partial class CameraPage : ContentPage
    {
        private bool isProcessing = false;
        private bool isScanLineAnimating = false;
        private CancellationTokenSource animationCancellationTokenSource;

        // Properties for layout calculations
        public double ScanWindowSize { get; private set; } = 250;
        public double TopOverlayHeight { get; private set; }
        public double BottomOverlayHeight { get; private set; }
        public double SideOverlayWidth { get; private set; }

        public CameraPage()
        {
            InitializeComponent();
            overlayGrid.BindingContext = this;

            // Add tap-to-focus capability
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnScanAreaTapped;
            scanFrame.GestureRecognizers.Add(tapGesture);
        }

        private async void OnScanAreaTapped(object sender, EventArgs e)
        {
            try
            {
                // Reset detection briefly to help with focus/scan issues
                scannerView.IsDetecting = false;
                await Task.Delay(200);
                scannerView.IsDetecting = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Focus error: {ex.Message}");
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Request camera permission
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required", "Camera permission is needed to scan barcodes", "OK");
                await Navigation.PopModalAsync();
                return;
            }

            // Ensure we're starting with a clean state
            isProcessing = false;

            try
            {
                // Configure the scanner with more specific options
                scannerView.Options = new BarcodeReaderOptions
                {
                    
                    AutoRotate = true,  // This is important for orientation handling
                    Multiple = false,
                    TryHarder = true   // Makes scanning more aggressive
                };

                // Make sure any device orientation works
                scannerView.CameraLocation = CameraLocation.Rear;

                // Start the scanner with a slight delay to ensure the UI is loaded
                await Task.Delay(500);

                // Stop and restart detection to apply new settings
                scannerView.IsDetecting = false;
                await Task.Delay(100);
                scannerView.IsDetecting = true;

                // Reset and start the animation
                StopScanLineAnimation();
                await Task.Delay(100);
                StartScanLineAnimation();

                // Add debug output
                Console.WriteLine("Camera scanner initialized and started");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Camera Error", $"Failed to initialize scanner: {ex.Message}", "OK");
                Console.WriteLine($"Camera error: {ex}");
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Calculate overlay dimensions
            if (width > 0 && height > 0)
            {
                // Make the scan window a bit smaller on smaller screens
                ScanWindowSize = Math.Min(width, height) * 0.7;

                // Calculate overlay dimensions
                SideOverlayWidth = (width - ScanWindowSize) / 2;
                double centerAreaTop = (height - ScanWindowSize) / 2;
                TopOverlayHeight = centerAreaTop;
                BottomOverlayHeight = centerAreaTop;

                // Force property change notification
                OnPropertyChanged(nameof(ScanWindowSize));
                OnPropertyChanged(nameof(TopOverlayHeight));
                OnPropertyChanged(nameof(BottomOverlayHeight));
                OnPropertyChanged(nameof(SideOverlayWidth));
            }
        }

        private void StartScanLineAnimation()
        {
            StopScanLineAnimation();

            animationCancellationTokenSource = new CancellationTokenSource();
            isScanLineAnimating = true;

            // Start the animation in a separate task
            Task.Run(async () => await AnimateScanLineAsync(animationCancellationTokenSource.Token));
        }

        private void StopScanLineAnimation()
        {
            isScanLineAnimating = false;
            animationCancellationTokenSource?.Cancel();
            animationCancellationTokenSource = null;
        }

        private async Task AnimateScanLineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // Move from top to bottom
                        await scanLine.TranslateTo(0, -ScanWindowSize / 2 + 2, 0);
                        if (!cancellationToken.IsCancellationRequested)
                            await scanLine.TranslateTo(0, ScanWindowSize / 2 - 2, 1500, Easing.Linear);

                        // Move from bottom to top
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await scanLine.TranslateTo(0, ScanWindowSize / 2 - 2, 0);
                            await scanLine.TranslateTo(0, -ScanWindowSize / 2 + 2, 1500, Easing.Linear);
                        }
                    });

                    // Small delay between animations
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation occurs
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Animation error: {ex.Message}");
            }
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            Console.WriteLine($"Barcode detection event triggered. Results: {e.Results?.Length ?? 0}");

            if (e.Results == null || e.Results.Length == 0 || isProcessing)
                return;

            var barcode = e.Results[0];
            if (string.IsNullOrWhiteSpace(barcode.Value))
                return;

            // Prevent multiple processing of the same barcode
            isProcessing = true;

            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Try to vibrate the device for feedback
                        if (Vibration.Default.IsSupported)
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                    }
                    catch { /* Ignore vibration errors */ }

                    // Stop scanning and animation
                    scannerView.IsDetecting = false;
                    StopScanLineAnimation();

                    Console.WriteLine($"Processing barcode: {barcode.Value}, Format: {barcode.Format}");

                    // Process the result
                    await ProcessScannedBarcodeAsync(barcode.Value);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Barcode processing error: {ex.Message}");
                isProcessing = false;
            }
        }


        private async Task ProcessScannedBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                // Add or update the scanned item
                await ItemService.AddOrUpdateItem(barcode);

                // Display success message
                await DisplayAlert("Success", $"Scanned barcode: {barcode}", "OK");

                // Close this page and return to the scan tab, but navigate to the items tab
                await Navigation.PopModalAsync();

                // Navigate to items tab
                await Shell.Current.GoToAsync("//items");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to process barcode: {ex.Message}", "OK");
            }
            finally
            {
                isProcessing = false;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            // Stop scanning and animation
            scannerView.IsDetecting = false;
            StopScanLineAnimation();

            // Go back to the scan page
            await Navigation.PopModalAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Ensure scanner and animation are stopped when page disappears default
            scannerView.IsDetecting = false;
            StopScanLineAnimation();
        }

        private async void OnManualEntryClicked(object sender, EventArgs e)
        {
            // Stop scanning and animation while dialog is open
            scannerView.IsDetecting = false;
            StopScanLineAnimation();

            string result = await DisplayPromptAsync("Manual Entry", "Enter barcode:", "OK", "Cancel");

            if (!string.IsNullOrWhiteSpace(result))
            {
                // Process the manually entered barcode
                await ProcessScannedBarcodeAsync(result);
            }
            else
            {
                // If canceled, restart scanning and animation
                scannerView.IsDetecting = true;
                StartScanLineAnimation();
            }
        }

    }
}
