using BepInEx.Configuration;
using System;

namespace NoMoreMath.Config
{
    public struct ConfigContext
    {
        public ConfigFile File;

        public string SectionName;

        public string NameFormat;

        public readonly string GetFormattedName(string name)
        {
            return string.IsNullOrEmpty(NameFormat) ? name : string.Format(NameFormat, name);
        }

        public readonly ConfigEntry<T> Bind<T>(string name, T defaultValue, ConfigDescription description)
        {
            if (File == null)
                throw new InvalidOperationException("ConfigFile not set");

            if (string.IsNullOrEmpty(SectionName))
                throw new InvalidOperationException("SectionName not set");

            return File.Bind(SectionName, GetFormattedName(name), defaultValue, description);
        }
    }
}
