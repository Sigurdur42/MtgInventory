using MtgBinder.Wpf.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CardInventoryControl.xaml
    /// </summary>
    public partial class CardInventorySingleControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty InventoryCardProperty =
            DependencyProperty.Register("InventoryCard", typeof(MtgInventoryCardViewModel),
                typeof(CardInventorySingleControl), new FrameworkPropertyMetadata(null, OnInventoryCardPropertyChanged));

        public CardInventorySingleControl()
        {
            InitializeComponent();
            rootGrid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string[] AvailableLanguages => AvailableCollection.AvailableLanguages();
        public MtgCondition[] AvailableConditions => AvailableCollection.AvailableConditions();

        public MtgInventoryCardViewModel InventoryCard
        {
            get => (MtgInventoryCardViewModel)GetValue(InventoryCardProperty);
            set
            {
                SetValue(InventoryCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InventoryCard)));
            }
        }

        private static void OnInventoryCardPropertyChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            // TODO: Access image cache
            // TODO: Display other image details

            ////var control = source as CardDetailsControl;
            ////if (control == null)
            ////{
            ////    return;
            ////}

            ////control.SetCard(control.SelectedCard);
        }
    }
}