using MtgBinder.Domain.Inventory;
using PropertyChanged;

namespace MtgBinder.Inventory
{
    [AddINotifyPropertyChangedInterface]
    public class InventoryItemViewModel
    {
        public InventoryItemViewModel(
            InventoryItem item)
        {
            Item = item;
        }

        public InventoryItem Item { get; }
    }
}