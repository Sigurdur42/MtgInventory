using System;
using Microsoft.Extensions.Logging;

namespace MtgInventoryBlazor
{
    public enum ToastCategory
    {
        Error,
        Warning,
        Info,
        Success,
    }

    public class RequestToastToDisplayEventArgs : EventArgs
    {
        public string Message { get; set; }
        public ToastCategory Category { get; set; }
        public string Header { get; set; }
    }
}