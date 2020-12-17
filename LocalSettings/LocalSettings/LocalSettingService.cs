using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LocalSettings
{
    public enum SettingWriteMode
    {
        OnChange,
        Manual
    }

    public interface ILocalSettingService
    {
        bool IsInitialized { get; }

        void Initialize(
            FileInfo settingFile,
            SettingWriteMode writeMode);

        string? Get(string key);
        int GetInt(string key);
        
        void Set(string key, string value);
        void Set(string key, int value);
        
        void WriteSettings();
    }

    public class LocalSettingService : ILocalSettingService
    {
        private readonly ISerializer _serializer;

        private readonly object _sync = new object();

        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public LocalSettingService()
        {
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        public FileInfo? SettingFile { get; private set; }
        public SettingWriteMode WriteMode { get; private set; } =  SettingWriteMode.OnChange;

        public bool IsInitialized { get; private set; }

        public void Initialize(
            FileInfo settingFile,
            SettingWriteMode writeMode)
        {
            lock (_sync)
            {
                WriteMode = writeMode;
                SettingFile = settingFile;
                var folder = settingFile.Directory;
                if (!(folder?.Exists ?? false))
                {
                    folder?.Create();
                }

                ReadSettings();

                IsInitialized = true;
            }
        }

        private void VerifyInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The setting service has not been initialized. Call the Initialize() method first.");
            }
        }

        public int GetInt(string key)
        {
            var found = Get(key);
            if (found == null)
            {
                return 0;
            }

            return int.TryParse(found, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue) ? intValue : 0;
        }
        public string? Get(string key)
        {
            VerifyInitialized();
            lock (_sync)
            {
                key = key?.ToUpperInvariant() ?? "";
                return _settings.ContainsKey(key) ? _settings[key] : null;
            }
        }

        public void Set(string key, int value)
        {
            Set(key, value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void Set(string key, string value)
        {
            VerifyInitialized();
            lock (_sync)
            {
                key = key?.ToUpperInvariant() ?? "";
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                if (!_settings.ContainsKey(key))
                {
                    _settings.Add(key, value);
                }
                else
                {
                    _settings[key] = value;
                }

                if (WriteMode == SettingWriteMode.OnChange)
                {
                    WriteSettings();
                }
            }
        }

        private void ReadSettings()
        {
            if (!(SettingFile?.Exists ?? false))
            {
                return;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var content = File.ReadAllText(SettingFile.FullName);
            _settings = deserializer.Deserialize<Dictionary<string, string>>(content);
        }

        public void WriteSettings()
        {
            var yaml = _serializer.Serialize(_settings);
            File.WriteAllText(SettingFile!.FullName, yaml);
        }
    }
}