using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace TeammateRevive.Configuration
{
    public static class ConfigFileExtensions
    {
        public static BindCollection BindCollection(this ConfigFile config, string section = null)
        {
            return new BindCollection(config, section);
        }
    }

    public class BindCollection
    {
        private readonly ConfigFile config;
        public string Section { get; private set; }

        public List<ConfigEntryBase> Bindings { get; } = new();
        private Dictionary<ConfigEntryBase, Action> UpdateCallbacks { get; } = new();

        public event Action OnChanged;

        public BindCollection(ConfigFile config, string section)
        {
            this.config = config;
            this.Section = section;
            this.config.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (UpdateCallbacks.TryGetValue(e.ChangedSetting, out var cb))
            {
                cb();
                OnChanged?.Invoke();
            }
        }

        public BindCollection Bind<TValue>(string key,
            string description,
            Action<TValue> set,
            TValue defaultValue = default)
        {
            var binding = this.config.Bind(this.Section, key, description: description,
                defaultValue: defaultValue);
            set(binding.Value);
            this.Bindings.Add(binding);
            UpdateCallbacks.Add(binding, () => set(binding.Value));
            return this;
        }

        public BindCollection Bind<TValue>(string key,
            ConfigDescription configDescription,
            Action<TValue> set,
            TValue defaultValue = default)
        {
            var binding = this.config.Bind(this.Section, key, defaultValue, configDescription);
            set(binding.Value);
            this.Bindings.Add(binding);
            UpdateCallbacks.Add(binding, () => set(binding.Value));
            return this;
        }
    }
}