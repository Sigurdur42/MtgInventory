using System.IO;
using System.Threading.Tasks;

namespace MtgJson.Sqlite
{
    public interface IMtgJsonMirrorIntoSqliteService
    {
        Task CreateLocalSqliteMirror(
            FileInfo targetFile,
            bool updatePriceDataOnly);
    }
}