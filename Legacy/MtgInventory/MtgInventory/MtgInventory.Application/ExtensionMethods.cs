using System.IO;

namespace MtgInventory.Service
{
    public static class ExtensionMethods
    {
        public static void EnsureExists(this DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                dir.Create();
            }
        }
    }
}