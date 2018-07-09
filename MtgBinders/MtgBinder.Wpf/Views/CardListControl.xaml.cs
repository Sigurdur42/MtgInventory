using MtgBinder.Wpf.ViewModels;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CardListControl.xaml
    /// </summary>
    public partial class CardListControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register("SelectedCard", typeof(MtgFullCardViewModel),
            typeof(CardListControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        public static readonly DependencyProperty CardsProperty =
            DependencyProperty.Register("Cards", typeof(IEnumerable<MtgFullCardViewModel>),
            typeof(CardListControl), new FrameworkPropertyMetadata(null, OnCardsPropertyChanged));

        public CardListControl()
        {
            InitializeComponent();

            rootGrid.DataContext = this;
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

        public IEnumerable<MtgFullCardViewModel> Cards
        {
            get => (IEnumerable<MtgFullCardViewModel>)GetValue(CardsProperty);
            set
            {
                SetValue(CardsProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cards)));
            }
        }

        private static void OnSelectedCardPropertyChanged(
           DependencyObject source,
           DependencyPropertyChangedEventArgs e)
        {
            ////var control = source as CardListControl;
            ////if (control == null)
            ////{
            ////    return;
            ////}

            ////control.SelectedCard = (MtgFullCard)e.NewValue;
        }

        private static void OnCardsPropertyChanged(
           DependencyObject source,
           DependencyPropertyChangedEventArgs e)
        {
            var control = source as CardListControl;
            if (control == null)
            {
                return;
            }

            if (control.SelectedCard != null)
            {
                var newValue = (IEnumerable<MtgFullCardViewModel>)e.NewValue;
                if (newValue != null)
                {
                    if (!newValue.Contains(control.SelectedCard))
                    {
                        control.SelectedCard = null;
                    }
                }
                else
                {
                    control.SelectedCard = null;
                }
            }
        }
    }
}