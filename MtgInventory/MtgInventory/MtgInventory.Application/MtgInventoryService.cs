using System;
using System.Collections.Generic;
using System.Text;

namespace MtgInventory.Service
{
    /// <summary>
    /// This is the main service class which encapsulates all functionality
    /// </summary>
    public class MtgInventoryService
    {
        public MtgInventoryService()
        {
            SystemFolders = new SystemFolders();
        }

        public SystemFolders SystemFolders { get; }

        public void Initialize()
        {
            // TODO: Implement async init 
            // Loading of database etc.
        }

    }
}
