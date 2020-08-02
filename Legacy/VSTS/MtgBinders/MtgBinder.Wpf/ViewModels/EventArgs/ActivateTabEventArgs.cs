using System;

namespace MtgBinder.Wpf.ViewModels
{
    internal class ActivateTabEventArgs : EventArgs
    {
        public ActivateTabEventArgs(int tabToBeActivated)
        {
            TabToBeActivated = tabToBeActivated;
        }
        public int TabToBeActivated { get; }
    }
}