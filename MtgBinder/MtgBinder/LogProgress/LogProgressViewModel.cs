using System.Windows;
using MtgBinder.Domain.Tools;
using PropertyChanged;
using Serilog.Core;
using Serilog.Events;

namespace MtgBinder.LogProgress
{
    [AddINotifyPropertyChangedInterface]
    public class LogProgressViewModel : ILogEventSink
    {
        public LogProgressViewModel()
        {
            ProgressBarVisibility = Visibility.Hidden;
        }

        public string LogMessage { get; set; }

        public Visibility ProgressBarVisibility { get; set; }

        public int Range { get; set; }

        public int CurrentStep { get; set; }

        public void Emit(LogEvent logEvent)
        {
            LogMessage = logEvent.RenderMessage();
        }

        public void UpdateProgressBar(ProgressEventArgs progressData)
        {
            Range = progressData.Range;
            CurrentStep = progressData.CurrentStep;

            ProgressBarVisibility = progressData.IsRunning
                ? ProgressBarVisibility = Visibility.Visible
                : ProgressBarVisibility = Visibility.Hidden;
        }
    }
}