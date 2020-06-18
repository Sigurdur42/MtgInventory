using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Logging
{
    /// <summary>
    /// Interaction logic for UiLoggingControl.xaml
    /// </summary>
    public partial class UiLoggingControl : UserControl
    {
        public UiLoggingControl()
        {
            InitializeComponent();

            LogItems = new ObservableCollection<string>();
            UiLogger.UiCallbacks.Add(AddLogItem);
            rootGrid.DataContext = this;
        }

        public ObservableCollection<string> LogItems { get; private set; }

        private void AddLogItem(DateTime timestamp, LogLevel logLevel, string category, string message, Exception error)
        {
            var builder = new StringBuilder();
            builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            builder.Append(" [");
            builder.Append(logLevel.ToString());
            builder.Append("] ");
            builder.Append(category);
            builder.Append(": ");
            builder.Append(message);

            if (error != null)
            {
                builder.AppendLine(error.ToString());
            }

            Dispatcher.Invoke(() =>
            {
                var lastItem = builder.ToString();
                LogItems.Add(lastItem);

                while (LogItems.Count > 2000)
                {
                    LogItems.RemoveAt(0);
                }
            });
        }
    }
}