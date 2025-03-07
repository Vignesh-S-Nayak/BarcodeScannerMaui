using ZXing.Net.Maui;
using BarcodeScannerMaui.Services;
using System.Timers;

namespace BarcodeScannerMaui.Views
{
    public partial class CameraPage : ContentPage
    {
        private bool isProcessing = false;
        private bool isScanLineAnimating = false;
        private CancellationTokenSource animationCancellationTokenSource;

       
        private System.Timers.Timer autoCloseTimer;
        private const int AUTO_CLOSE_TIMEOUT = 20000; 

      
        public double ScanWindowSize { get; private set; } = 250;
        public double TopOverlayHeight { get; private set; }
        public double BottomOverlayHeight { get; private set; }
        public double SideOverlayWidth { get; private set; }

        public CameraPage()
        {
            InitializeComponent();
            overlayGrid.BindingContext = this;

           
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnScanAreaTapped;
            scanFrame.GestureRecognizers.Add(tapGesture);

            
            SetupAutoCloseTimer();
        }

        private void SetupAutoCloseTimer()
        {
            
            autoCloseTimer = new System.Timers.Timer(AUTO_CLOSE_TIMEOUT);
            autoCloseTimer.AutoReset = false; 
            autoCloseTimer.Elapsed += AutoCloseTimerElapsed;
        }

        private void AutoCloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isProcessing) 
                {
                   
                    await DisplayAlert("Scanner Timeout", "Scanner closed due to inactivity.", "OK");

                    
                    scannerView.IsDetecting = false;
                    StopScanLineAnimation();
                    await Navigation.PopModalAsync();
                }
            });
        }

        
        private void ResetAutoCloseTimer()
        {
            autoCloseTimer.Stop();
            autoCloseTimer.Start();
        }

        private async void OnScanAreaTapped(object sender, EventArgs e)
        {
            try
            {
               
                scannerView.IsDetecting = false;
                await Task.Delay(200);
                scannerView.IsDetecting = true;

               
                ResetAutoCloseTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Focus error: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

           
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required", "Camera permission is needed to scan barcodes", "OK");
                await Navigation.PopModalAsync();
                return;
            }

           
            isProcessing = false;

            try
            {
               
                scannerView.Options = new BarcodeReaderOptions
                {
                    AutoRotate = true,  
                    Multiple = false,
                    TryHarder = true   
                };

       
                scannerView.CameraLocation = CameraLocation.Rear;

         
                await Task.Delay(500);

            
                scannerView.IsDetecting = false;
                await Task.Delay(100);
                scannerView.IsDetecting = true;

         
                StopScanLineAnimation();
                await Task.Delay(100);
                StartScanLineAnimation();

                autoCloseTimer.Start();

           
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

       
            if (width > 0 && height > 0)
            {

                ScanWindowSize = Math.Min(width, height) * 0.7;

       
                SideOverlayWidth = (width - ScanWindowSize) / 2;
                double centerAreaTop = (height - ScanWindowSize) / 2;
                TopOverlayHeight = centerAreaTop;
                BottomOverlayHeight = centerAreaTop;

               
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
                       
                        await scanLine.TranslateTo(0, -ScanWindowSize / 2 + 2, 0);
                        if (!cancellationToken.IsCancellationRequested)
                            await scanLine.TranslateTo(0, ScanWindowSize / 2 - 2, 1500, Easing.Linear);

                    
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await scanLine.TranslateTo(0, ScanWindowSize / 2 - 2, 0);
                            await scanLine.TranslateTo(0, -ScanWindowSize / 2 + 2, 1500, Easing.Linear);
                        }
                    });

                   
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
           
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

            isProcessing = true;

   
            autoCloseTimer.Stop();

            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        
                        if (Vibration.Default.IsSupported)
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                    }
                    catch { }

                    scannerView.IsDetecting = false;
                    StopScanLineAnimation();

                    Console.WriteLine($"Processing barcode: {barcode.Value}, Format: {barcode.Format}");

                    
                    await ProcessScannedBarcodeAsync(barcode.Value);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Barcode processing error: {ex.Message}");
                isProcessing = false;

                
                autoCloseTimer.Start();
            }
        }


        private async Task ProcessScannedBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                
                await ItemService.AddOrUpdateItem(barcode);

                
                await DisplayAlert("Success", $"Scanned barcode: {barcode}", "OK");

       
                await Navigation.PopModalAsync();

                
                await Shell.Current.GoToAsync("//items");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to process barcode: {ex.Message}", "OK");

               
                autoCloseTimer.Start();
                isProcessing = false;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            
            autoCloseTimer.Stop();

            
            scannerView.IsDetecting = false;
            StopScanLineAnimation();

           
            await Navigation.PopModalAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            
            scannerView.IsDetecting = false;
            StopScanLineAnimation();

            
            autoCloseTimer.Stop();
            autoCloseTimer.Dispose();
        }

        private async void OnManualEntryClicked(object sender, EventArgs e)
        {
            
            autoCloseTimer.Stop();

            
            scannerView.IsDetecting = false;
            StopScanLineAnimation();

            string result = await DisplayPromptAsync("Manual Entry", "Enter barcode:", "OK", "Cancel");

            if (!string.IsNullOrWhiteSpace(result))
            {
                
                await ProcessScannedBarcodeAsync(result);
            }
            else
            {
                
                scannerView.IsDetecting = true;
                StartScanLineAnimation();
                autoCloseTimer.Start();
            }
        }
    }
}
