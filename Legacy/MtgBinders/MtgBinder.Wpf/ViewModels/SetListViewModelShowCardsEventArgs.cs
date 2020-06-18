using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.ViewModels
{
    public class SetListViewModelShowCardsEventArgs : EventArgs
    {
        public string SetCode { get; set; }
    }
}