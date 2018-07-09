using System.ComponentModel;
using System.Windows.Controls;
using MtgBinder.Wpf.ViewModels;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SetListView.xaml
    /// </summary>
    public partial class SetListView : UserControl
    {
        public SetListView()
        {
            InitializeComponent();

            rootGrid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SetListViewModel SetListViewModel => DataContext as SetListViewModel;
    }
}