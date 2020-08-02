using System;

namespace MtgBinder.Domain.Tools
{
    public interface IAsyncProgress
    {
        event EventHandler<ProgressEventArgs> Starting;

        event EventHandler<ProgressEventArgs> Finishing;

        event EventHandler<ProgressEventArgs> Progress;
    }
}