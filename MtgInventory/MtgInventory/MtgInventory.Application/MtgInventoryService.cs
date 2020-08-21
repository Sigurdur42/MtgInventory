using MkmApi;
using MtgInventory.Service.Database;
using System;
using System.IO;

namespace MtgInventory.Service
{
    /// <summary>
    /// This is the main service class which encapsulates all functionality
    /// </summary>
    public sealed class MtgInventoryService : IDisposable
    {
        private readonly CardDatabase _cardDatabase;
        private MkmRequest _mkmRequest;
        private FileInfo _mkmAuthenticationDataFile;

        public MtgInventoryService()
        {
            SystemFolders = new SystemFolders();
            _cardDatabase = new CardDatabase();
        }

        public SystemFolders SystemFolders { get; }

        public MkmAuthenticationData MkmAuthenticationData { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize()
        {
            var reader = new AuthenticationReader();

            _mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _mkmRequest = new MkmRequest();

            _cardDatabase.Initialize(SystemFolders.BaseFolder);

            // TODO: Implement async init
            // Loading of database etc.


            // TODO: Implement reading all products from MKM and put it into database
        }

        public void ShutDown()
        {
            _cardDatabase.Dispose();
        }
    }
}