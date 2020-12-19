namespace MtgInventoryBlazor
{
    public interface IRequestToastMessage
    {
        void RequestToastError(string message, string header);
        void RequestToastWarning(string message, string header);
        void RequestToastSuccess(string message, string header);
        void RequestToastInfo(string message, string header);
    }
}