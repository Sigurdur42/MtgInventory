using Microsoft.Extensions.DependencyInjection;
using MtgBinder.Wpf.ViewModels;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Views
{
    public partial class CardInventoryCollectionControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register("SelectedCard", typeof(MtgFullCardViewModel),
                typeof(CardInventoryCollectionControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        private static IMtgInventoryService _inventoryService;

        public CardInventoryCollectionControl()
        {
            InitializeComponent();
            rootGrid.DataContext = this;

            if (_inventoryService == null)
            {
                _inventoryService = ApplicationSingeltons.ServiceProvider.GetService<IMtgInventoryService>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MtgInventoryCardViewModel> InventoryCards { get; } =
            new ObservableCollection<MtgInventoryCardViewModel>();

        public MtgFullCardViewModel SelectedCard
        {
            get => (MtgFullCardViewModel)GetValue(SelectedCardProperty);
            set
            {
                SetValue(SelectedCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));
            }
        }

        private static void OnSelectedCardPropertyChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var control = source as CardInventoryCollectionControl;

            control?.SetCard(control.SelectedCard);
        }

        private void SetCard(MtgFullCardViewModel card)
        {
            InventoryCards.Clear();
            if (card?.FullCard?.UniqueId == null)
            {
                return;
            }

            var existingInventory = _inventoryService.Cards.Where(c => c.CardId == card.FullCard.UniqueId).ToArray();
            foreach (var mtgInventoryCard in existingInventory)
            {
                InventoryCards.Add(new MtgInventoryCardViewModel(mtgInventoryCard));
            }

            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardSetInfo)));
        }

        private void OnAddInventory(object sender, RoutedEventArgs e)
        {
            var newInventory = new MtgInventoryCard(SelectedCard.FullCard)
            {
                Quantity = 1,
                LanguageCode = "EN"
            };

            _inventoryService.Cards.Add(newInventory);
            InventoryCards.Add(new MtgInventoryCardViewModel(newInventory));

            _inventoryService.SaveInventory();
        }
    }
}