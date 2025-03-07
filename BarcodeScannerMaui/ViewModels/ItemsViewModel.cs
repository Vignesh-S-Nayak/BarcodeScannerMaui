using System.Collections.ObjectModel;
using System.Windows.Input;
using BarcodeScannerMaui.Models;
using BarcodeScannerMaui.Services;

namespace BarcodeScannerMaui.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private ObservableCollection<Item> items;

        public ObservableCollection<Item> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        public ICommand ClearCommand { get; }
        public ICommand RemoveItemCommand { get; }

        public ItemsViewModel()
        {
            Title = "Scanned Items";
            ClearCommand = new Command(async () => await ClearItems());
            RemoveItemCommand = new Command<string>(async (barcode) => await RemoveItem(barcode));
        }

        public async Task OnAppearing()
        {
            await LoadItems();
        }

        public async Task LoadItems()
        {
            IsBusy = true;
            Items = await ItemService.GetItems();
            IsBusy = false;
        }

        public async Task ClearItems()
        {
            IsBusy = true;
            await ItemService.ClearAllItems();
            await LoadItems();
            IsBusy = false;
        }

        public async Task RemoveItem(string barcode)
        {
            IsBusy = true;
            await ItemService.RemoveItem(barcode);
            await LoadItems();
            IsBusy = false;
        }
    }
}
