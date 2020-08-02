using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinder.Wpf.ViewModels
{
    public class MtgInventoryCardViewModel : INotifyPropertyChanged
    {
        public MtgInventoryCardViewModel(
            MtgInventoryCard card)
        {
            InventoryCard = card;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MtgInventoryCard InventoryCard { get; }

        public int Quantity
        {
            get => InventoryCard.Quantity;
            set
            {
                if (value != InventoryCard.Quantity)
                {
                    InventoryCard.Quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                }
            }
        }

        public bool IsFoil
        {
            get => InventoryCard.IsFoil;
            set
            {
                if (value != InventoryCard.IsFoil)
                {
                    InventoryCard.IsFoil = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFoil)));
                }
            }
        }

        public MtgCondition Condition
        {
            get => InventoryCard.Condition;
            set
            {
                if (value != InventoryCard.Condition)
                {
                    InventoryCard.Condition = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Condition)));
                }
            }
        }

        public string LanguageCode
        {
            get => InventoryCard.LanguageCode;
            set
            {
                if (value != InventoryCard.LanguageCode)
                {
                    InventoryCard.LanguageCode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LanguageCode)));
                }
            }
        }
    }
}