using System.IO;

namespace MtgBinder.Domain.Configuration
{
    public interface IUserDataFolderProvider
    {
        DirectoryInfo ConfigurationFolder { get; }
    }
}