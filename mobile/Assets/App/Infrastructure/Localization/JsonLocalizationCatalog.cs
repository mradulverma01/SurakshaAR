using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SurakshaAR.Infrastructure.Localization
{
    public sealed class JsonLocalizationCatalog
    {
        private readonly string directory;
        private readonly Dictionary<string, LocalizationDocument> cache =
            new Dictionary<string, LocalizationDocument>(StringComparer.OrdinalIgnoreCase);

        public JsonLocalizationCatalog(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("A localization directory is required.", nameof(directory));
            }

            this.directory = directory;
        }

        public async Task<string> Get(string locale, string key)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                throw new ArgumentException("A locale is required.", nameof(locale));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A localization key is required.", nameof(key));
            }

            var document = await Load(locale).ConfigureAwait(false);
            if (document.Strings.TryGetValue(key, out var value))
            {
                return value;
            }

            if (!string.IsNullOrWhiteSpace(document.FallbackLocale))
            {
                var fallback = await Load(document.FallbackLocale).ConfigureAwait(false);
                if (fallback.Strings.TryGetValue(key, out value))
                {
                    return value;
                }
            }

            throw new KeyNotFoundException("No localized value exists for " + key + ".");
        }

        private async Task<LocalizationDocument> Load(string locale)
        {
            if (cache.TryGetValue(locale, out var cached))
            {
                return cached;
            }

            var path = Path.Combine(directory, locale + ".json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The locale is not installed.", path);
            }

            string json;
            using (var reader = new StreamReader(path))
            {
                json = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            var document = JsonConvert.DeserializeObject<LocalizationDocument>(json)
                ?? throw new InvalidDataException("The localization document is invalid.");
            cache[locale] = document;
            return document;
        }

        public sealed class LocalizationDocument
        {
            public string Locale { get; set; } = string.Empty;

            public string? FallbackLocale { get; set; }

            public Dictionary<string, string> Strings { get; set; } =
                new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
