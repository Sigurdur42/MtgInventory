using Microsoft.Extensions.DependencyInjection;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CardWantsControl.xaml
    /// </summary>
    public partial class CardWantsControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
            DependencyProperty.Register("SelectedCard", typeof(MtgFullCard),
            typeof(CardWantsControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        private readonly IMtgWantsListService _wantsService;

        private MtgWantListCard _want;

        public CardWantsControl()
        {
            InitializeComponent();
            rootGrid.DataContext = this;
            _wantsService = ApplicationSingeltons.ServiceProvider.GetService<IMtgWantsListService>();
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

        public MtgWantListCard WantCard
        {
            get => _want;
            set
            {
                if (_want != value)
                {
                    _want = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WantCard)));
                    FireCanChanged();
                }
            }
        }

        public int WantCount => _want?.WantCount ?? 0;

        public bool CanIncrease => SelectedCard != null;

        public bool CanDecrease => SelectedCard != null && _want != null && _want.WantCount > 0;

        private static void OnSelectedCardPropertyChanged(
          DependencyObject source,
          DependencyPropertyChangedEventArgs e)
        {
            var control = source as CardWantsControl;
            if (control == null)
            {
                return;
            }

            control.SetCard(control.SelectedCard);
        }

        private void FireCanChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanIncrease)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDecrease)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WantCount)));
        }

        private void SetCard(MtgFullCard card)
        {
            if (card == null)
            {
                WantCard = null;
                return;
            }

            WantCard = _wantsService.Wants.FirstOrDefault(w => w.CardId.Equals(card.UniqueId));
        }

        private void OnAddWant(object sender, RoutedEventArgs e)
        {
            _want = _wantsService.AddWant(SelectedCard, 1);
            FireCanChanged();
        }

        private void OnRemoveWant(object sender, RoutedEventArgs e)
        {
            if (!CanDecrease)
            {
                return;
            }

            _want = _wantsService.AddWant(SelectedCard, -1);
            FireCanChanged();
        }
    }
}