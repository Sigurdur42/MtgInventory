using MtgBinder.Wpf.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CardPriceControl.xaml
    /// </summary>
    public partial class CardPriceControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register("SelectedCard", typeof(MtgFullCardViewModel),
            typeof(CardPriceControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        private bool _displayTix;

        public CardPriceControl()
        {
            DisplayTix = true;
            InitializeComponent();
            rootGrid.DataContext = this;
            SetCard(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MtgFullCardViewModel SelectedCard
        {
            get => (MtgFullCardViewModel)GetValue(SelectedCardProperty);
            set
            {
                SetValue(SelectedCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));
            }
        }

        public string PriceEur { get; private set; }
        public string PriceUsd { get; private set; }

        public string PriceTix { get; private set; }

        public Visibility DisplayTixVisibility { get; private set; }

        public bool DisplayTix
        {
            get => _displayTix;

            set
            {
                if (value != _displayTix)
                {
                    _displayTix = value;
                    DisplayTixVisibility = _displayTix ? Visibility.Visible : Visibility.Collapsed;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTix)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTixVisibility)));
                }
            }
        }

        private static void OnSelectedCardPropertyChanged(
                 DependencyObject source,
                    DependencyPropertyChangedEventArgs e)
        {
            var control = source as CardPriceControl;

            if (e.OldValue is MtgFullCardViewModel oldCard && control != null)
            {
                oldCard.PropertyChanged -= control.Card_PropertyChanged;
            }

            control?.SetCard(control.SelectedCard);
        }

        private void FireCanChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceEur)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceUsd)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceTix)));
        }

        private void SetCard(MtgFullCardViewModel card)
        {
            if (card != null)
            {
                card.PropertyChanged += Card_PropertyChanged;
            }

            UpdatePrice();
        }

        private void UpdatePrice()
        {
            var card = SelectedCard;
            PriceEur = (card?.PriceEur?.ToString("F2") ?? "---") + "€";
            PriceUsd = (card?.PriceUsd?.ToString("F2") ?? "---") + "$";
            PriceTix = (card?.PriceTix?.ToString("F2") ?? "---") + "Tix";
            FireCanChanged();
        }

        private void Card_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PriceEur):
                case nameof(PriceUsd):
                case nameof(PriceTix):
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdatePrice();
                        PropertyChanged?.Invoke(sender, e);
                    });
                    break;
            }
        }
    }
}