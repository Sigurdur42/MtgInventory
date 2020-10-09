using System;
using System.IO;
using System.Linq;
using LiteDB;
using Serilog;

namespace MtgInventory.Service.Settings
{
    public interface ISettingsService : IDisposable
    {
        MtgInventorySettings Settings { get; }

        void Initialize(
            DirectoryInfo folder);

        void SaveSettings();
    }

    public sealed class SettingsService : ISettingsService
    {
        private readonly ILogger _logger = Log.ForContext<SettingsService>();
        private LiteDatabase? _database;

        private ILiteCollection<MtgInventorySettings>? _settingsCollection;

        public MtgInventorySettings Settings { get; private set; } = new MtgInventorySettings();

        public void Dispose()
        {
            _database?.Dispose();
        }

        public void Initialize(
            DirectoryInfo folder)
        {
            _logger.Information($"Initializing setting service...");
            folder.EnsureExists();

            var databaseFile = Path.Combine(folder.FullName, "Settings.db");
            _database = new LiteDatabase(databaseFile);

            _settingsCollection = _database.GetCollection<MtgInventorySettings>();

            var found = _settingsCollection.FindAll().FirstOrDefault();
            if (found == null)
            {
                found = new MtgInventorySettings();
                _settingsCollection.Insert(found);
            }

            Settings = found;
        }

        public void SaveSettings()
        {
            _settingsCollection?.Update(Settings);
        }
    }
}