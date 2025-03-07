using System.Collections.ObjectModel;
using BarcodeScannerMaui.Models;

namespace BarcodeScannerMaui.Services
{
    public static class ItemService
    {
        private static ObservableCollection<Item> items = new ObservableCollection<Item>();

        public static Task<ObservableCollection<Item>> GetItems()
        {
            return Task.FromResult(items);
        }

        public static Task AddOrUpdateItem(string barcode)
        {
            barcode = barcode?.Trim();
            var existingItem = items.FirstOrDefault(i => i.Barcode == barcode);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                items.Add(new Item
                {
                    Barcode = barcode,
                    Name = $"Product {barcode}",
                    Quantity = 1
                });
            }

            return Task.CompletedTask;
        }

        public static Task RemoveItem(string barcode)
        {
            barcode = barcode?.Trim();
            var item = items.FirstOrDefault(i => i.Barcode == barcode);

            if (item != null)
            {
                items.Remove(item);
            }

            return Task.CompletedTask;
        }

        public static Task ClearAllItems()
        {
            items.Clear();
            return Task.CompletedTask;
        }
    }
}
