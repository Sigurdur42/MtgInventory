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
    /// Interaction logic for SetDetailsControl.xaml
    /// </summary>
    public partial class SetDetailsControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register("SelectedSet", typeof(MtgSetInfo),
            typeof(SetDetailsControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        public SetDetailsControl()
        {
            InitializeComponent();
            rootGrid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MtgSetInfo SelectedSet
        {
            get { return (MtgSetInfo)GetValue(SelectedCardProperty); }
            set
            {
                SetValue(SelectedCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSet)));
            }
        }

        private static void OnSelectedCardPropertyChanged(
          DependencyObject source,
          DependencyPropertyChangedEventArgs e)
        {
        }
    }
}