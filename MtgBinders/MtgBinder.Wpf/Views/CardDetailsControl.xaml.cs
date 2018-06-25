using Microsoft.Extensions.DependencyInjection;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Services.Images;
using MtgBinders.Domain.ValueObjects;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CardDetailsControl.xaml
    /// </summary>
    public partial class CardDetailsControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedCardProperty =
             DependencyProperty.Register("SelectedCard", typeof(MtgFullCard),
             typeof(CardDetailsControl), new FrameworkPropertyMetadata(null, OnSelectedCardPropertyChanged));

        private readonly IMtgImageCache _imageCache;
        private readonly IMtgSetRepository _setRepository;

        public CardDetailsControl()
        {
            InitializeComponent();
            _imageCache = ApplicationSingeltons.ServiceProvider.GetService<IMtgImageCache>();
            _setRepository = ApplicationSingeltons.ServiceProvider.GetService<IMtgSetRepository>();

            rootGrid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string CardImage { get; private set; }

        public MtgFullCard SelectedCard
        {
            get { return (MtgFullCard)GetValue(SelectedCardProperty); }
            set
            {
                SetValue(SelectedCardProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));
            }
        }

        public MtgSetInfo CardSetInfo { get; private set; }

        private static void OnSelectedCardPropertyChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            // TODO: Access image cache
            // TODO: Display other image details

            var control = source as CardDetailsControl;
            if (control == null)
            {
                return;
            }

            control.SetCard(control.SelectedCard);
        }

        private void SetCard(MtgFullCard card)
        {
            CardImage = _imageCache.GetImageFile(card);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardImage)));

            CardSetInfo = card != null
                ? _setRepository.SetData.FirstOrDefault(s => s.SetCode.Equals(card.SetCode, System.StringComparison.InvariantCultureIgnoreCase))
                : null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardSetInfo)));
        }
    }
}