using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.ViewModels
{
    public class MtgFullCardViewModel
    {
        public MtgFullCardViewModel(MtgFullCard fullCard)
        {
            FullCard = fullCard;
        }

        public MtgFullCard FullCard { get; }
    }
}