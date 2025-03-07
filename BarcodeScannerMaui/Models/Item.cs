using System.ComponentModel;

namespace BarcodeScannerMaui.Models
{
    public class Item : INotifyPropertyChanged
    {
        public string Barcode { get; set; }
        public string Name { get; set; }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
