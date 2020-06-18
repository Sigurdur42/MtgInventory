namespace MtgBinder.Domain.Tools
{
    public interface IAsyncProgressNotifier
    {
        void Start(string action, int range);
        void NextStep(string action);
        void Finish(string action);
    }
}