using MtgBinders.Domain.ValueObjects;
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
            DependencyProperty.Register("SelectedCard", typeof(MtgFullCard),
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

        public MtgFullCard SelectedCard
        {
            get { return (MtgFullCard)GetValue(SelectedCardProperty); }
            set
            {
                SetValue(SelectedCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));
            }
        }

        public string PriceEur { get; private set; }

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
            if (control == null)
            {
                return;
            }

            control.SetCard(control.SelectedCard);
        }

        private void FireCanChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceEur)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceTix)));
        }

        private void SetCard(MtgFullCard card)
        {
            PriceEur = card?.PriceEur?.ToString("F2") ?? "---";
            PriceTix = card?.PriceTix?.ToString("F2") ?? "---";
            FireCanChanged();
        }
    }
}