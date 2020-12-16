using System;
using System.IO;
using System.Linq;
using LiteDB;

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
        private LiteDatabase? _database;

        private ILiteCollection<MtgInventorySettings>? _settingsCollection;

        public MtgInventorySettings Settings { get; private set; } = new MtgInventorySettings();

        public void Dispose()
        {
            _database?.Dispose();
            _settingsCollection = null;
        }

        public void Initialize(
            DirectoryInfo folder)
        {
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

            if (found.EnsureValidSettings())
            {
                _settingsCollection.Update(found);
            }

            Settings = found;
        }

        public void SaveSettings()
        {
            _settingsCollection?.Update(Settings);
        }
    }
}