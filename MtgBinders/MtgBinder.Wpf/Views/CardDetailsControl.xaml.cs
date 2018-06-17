using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CardDetailsControl.xaml
    /// </summary>
    public partial class CardDetailsControl : UserControl
    {
        public static readonly DependencyProperty SelectedCardProperty =
             DependencyProperty.Register("SelectedCard", typeof(MtgFullCard),
             typeof(CardDetailsControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        public CardDetailsControl()
        {
            InitializeComponent();
        }

        // .NET Property wrapper
        public MtgFullCard SelectedCard
        {
            get { return (MtgFullCard)GetValue(SelectedCardProperty); }
            set { SetValue(SelectedCardProperty, value); }
        }

        private static void OnSelectedCardPropertyChanged(DependencyObject source,
                DependencyPropertyChangedEventArgs e)
        {
            // TODO: Access image cache
            // TODO: Display other image details

            var control = source as CardDetailsControl;
            // DateTime time = (DateTime)e.NewValue; Put some update logic here...
        }
    }
}