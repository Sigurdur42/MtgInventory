using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MtgBinder.Domain.Database;
using PropertyChanged;

namespace MtgBinder.Inventory
{
    [AddINotifyPropertyChangedInterface]

    public class InventoryViewModel
    {
        private readonly ICardDatabase _database;

        public InventoryViewModel(
            ICardDatabase database)
        {
            _database = database;

            _database.DatabaseInitialized += (sender, e) => ReloadInventory();
            _database.InventoryChanged += (sender, e) => ReloadInventory();
        }

        public ObservableCollection<InventoryItemViewModel> Items { get; set; }

        private void ReloadInventory()
        {
            
        }
    }
}
