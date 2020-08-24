namespace MtgInventory
{
    public static class MessageBoxDialog
    {
        public static void DisplayError(
            string title,
            string content)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                title,
                content,
                icon: MessageBox.Avalonia.Enums.Icon.Error);
            messageBoxStandardWindow.Show();
        }
    }
}