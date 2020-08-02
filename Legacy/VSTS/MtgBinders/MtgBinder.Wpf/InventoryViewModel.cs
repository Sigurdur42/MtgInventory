using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MtgBinder.Wpf.ViewModels;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinder.Wpf
{
    internal class InventoryViewModel
    {
        private readonly IMtgInventoryService _inventoryService;

        public InventoryViewModel(
            IMtgInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _inventoryService.Initialized += (sender, e) => UpdateCollection();

            InventoryCards = new ObservableCollection<MtgInventoryCardViewModel>();
        }

        public ObservableCollection<MtgInventoryCardViewModel> InventoryCards { get; private set; }

        public MtgInventoryCardViewModel AddCard(MtgFullCardViewModel card)
        {
            var inventoryCard = new MtgInventoryCard(card.FullCard);
            _inventoryService.Cards.Add(inventoryCard);
            _inventoryService.SaveInventory();

            var viewModel = new MtgInventoryCardViewModel(inventoryCard);
            InventoryCards.Add(viewModel);
            return viewModel;
        }

        public void SaveInventory()
        {
            _inventoryService.SaveInventory();
        }

        internal void UpdateCollection()
        {
            // TODO: Optimize this
            InventoryCards.Clear();
            foreach (var inventoryServiceCard in _inventoryService.Cards)
            {
                InventoryCards.Add(new MtgInventoryCardViewModel(inventoryServiceCard));
            }
        }
    }
}