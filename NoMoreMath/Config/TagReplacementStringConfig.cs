using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NoMoreMath.Config
{
    public class TagReplacementStringConfig : IDisposable
    {
        readonly struct TagInfo
        {
            public readonly string Tag;
            public readonly int StartIndex;

            public TagInfo(string tag, int startIndex)
            {
                Tag = tag;
                StartIndex = startIndex;
            }
        }

        public readonly struct ReplacementInfo
        {
            public readonly string Tag;
            public readonly string Value;

            public ReplacementInfo(string tag, string value)
            {
                Tag = tag;
                Value = value;
            }
        }

        public readonly ConfigEntry<string> ConfigEntry;

        readonly string[] _validTags;

        public int StrippedLength { get; private set; }

        TagInfo[] _cachedTagInfos = [];

        public event Action OnValueChanged;

        public TagReplacementStringConfig(ConfigEntry<string> configEntry, string[] validTags)
        {
            ConfigEntry = configEntry;
            _validTags = validTags;

            ConfigEntry.SettingChanged += ConfigEntry_SettingChanged;
            parseConfigValue();
        }

        public void Dispose()
        {
            ConfigEntry.SettingChanged -= ConfigEntry_SettingChanged;
        }

        void ConfigEntry_SettingChanged(object sender, EventArgs e)
        {
            parseConfigValue();
            OnValueChanged?.Invoke();
        }

        void parseConfigValue()
        {
            List<TagInfo> tagInfos = new List<TagInfo>(_validTags.Length);

            string str = ConfigEntry.Value;

            for (int i = 0; i < str.Length; i++)
            {
                foreach (string tag in _validTags)
                {
                    if (i + tag.Length > str.Length)
                        continue;

                    if (string.Compare(str, i, tag, 0, tag.Length, CultureInfo.InvariantCulture, CompareOptions.None) == 0)
                    {
                        tagInfos.Add(new TagInfo(tag, i));
                        i += tag.Length - 1;
                        break;
                    }
                }
            }

            _cachedTagInfos = tagInfos.ToArray();

            int strippedLength = str.Length;
            foreach (TagInfo formatInfo in _cachedTagInfos)
            {
                strippedLength -= formatInfo.Tag.Length;
            }

            StrippedLength = strippedLength;

#if DEBUG
            Log.Debug($"Refreshed config format string '{ConfigEntry.Definition}': StrippedLength={StrippedLength}, tagInfos=[{string.Join(", ", _cachedTagInfos.GroupBy(f => f.Tag).Select(g => $"{g.Key}: [{string.Join(", ", g.Select(f => f.StartIndex))}]"))}]");
#endif
        }

        public void AppendToStringBuilder(StringBuilder stringBuilder, params ReplacementInfo[] tagReplacements)
        {
            string configValue = ConfigEntry.Value;

            if (_cachedTagInfos.Length == 0)
            {
                stringBuilder.Append(configValue);
                return;
            }

            int startIndex = 0;

            foreach (TagInfo tagInfo in _cachedTagInfos)
            {
                int prefixLength = tagInfo.StartIndex - startIndex;
                if (prefixLength > 0)
                {
                    stringBuilder.Append(configValue, startIndex, prefixLength);
                }

                startIndex = tagInfo.StartIndex + tagInfo.Tag.Length;

                foreach (ReplacementInfo tagReplacement in tagReplacements)
                {
                    if (string.Equals(tagReplacement.Tag, tagInfo.Tag))
                    {
                        stringBuilder.Append(tagReplacement.Value);
                        break;
                    }
                }
            }

            int remainingChars = configValue.Length - startIndex;
            if (remainingChars > 0)
            {
                stringBuilder.Append(configValue, startIndex, remainingChars);
            }
        }
    }
}
