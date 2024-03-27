using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Serialization;

namespace StardewModdingAPI.Toolkit.Utilities
{
    /// <summary>Provides utilities for I18N.</summary>
    public static class I18nUtilities
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The path to the smapi-internal folder.</summary>
        private static readonly string InternalFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        /// <summary>The path to the i18n folder.</summary>
        private static readonly string I18NFolder = Path.Combine(InternalFolder, "i18n");

        private const string DefaultLocale = "default";

        /*********
        ** Fields
        *********/

        /// <summary>The translations for each locale.</summary>
        private static Dictionary<string, IDictionary<string, string>>  translations = new Dictionary<string, IDictionary<string, string>>();

        /// <summary>Current locale.</summary>
        private static string locale = DefaultLocale;

        /*********
        ** Public methods
        *********/

        /// <summary>Read translations from a directory containing JSON translation files.</summary>
        public static void Init()
        {
            JsonHelper jsonHelper = new();

            DirectoryInfo translationsDir = new(I18NFolder);
            if (translationsDir.Exists)
            {
                foreach (FileInfo file in translationsDir.EnumerateFiles("*.json"))
                {
                    string localeFileName = Path.GetFileNameWithoutExtension(file.Name.ToLower().Trim());
                    try
                    {
                        if (!jsonHelper.ReadJsonFileIfExists(file.FullName, out IDictionary<string, string>? data))
                        {
                            continue;
                        }
                        translations[localeFileName] = data;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            CultureInfo culture = CultureInfo.InstalledUICulture;

            // Get the ISO code of a language
            locale = culture.TwoLetterISOLanguageName.ToLower();
        }

        /// <summary>Get I18N text</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public static string Get(string key, object? tokens)
        {
            if (locale==null||!translations.ContainsKey(locale))
            {
                throw new Exception("I18N Translation not loading properly.");
            }

            return ReplaceTokens(translations[locale][key], tokens);
        }

        /// <summary>Get default I18N text</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public static string GetDefault(string key, object? tokens)
        {
            if (!translations.ContainsKey(DefaultLocale))
            {
                throw new Exception("I18N Translation not loading properly.");
            }

            return ReplaceTokens(translations[DefaultLocale][key], tokens);
        }

        /// <summary>Replace tokens in the text like <c>{{value}}</c> with the given values. Returns a new instance.</summary>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static string ReplaceTokens(string text, object? tokens)
        {
            if (string.IsNullOrWhiteSpace(text) || tokens == null)
                return text;

            // get dictionary of tokens
            Dictionary<string, string?> tokenLookup = new(StringComparer.OrdinalIgnoreCase);
            {
                // from dictionary
                if (tokens is IDictionary inputLookup)
                {
                    foreach (DictionaryEntry entry in inputLookup)
                    {
                        string? key = entry.Key.ToString()?.Trim();
                        if (key != null)
                            tokenLookup[key] = entry.Value?.ToString();
                    }
                }

                // from object properties
                else
                {
                    Type type = tokens.GetType();
                    foreach (PropertyInfo prop in type.GetProperties())
                        tokenLookup[prop.Name] = prop.GetValue(tokens)?.ToString();
                    foreach (FieldInfo field in type.GetFields())
                        tokenLookup[field.Name] = field.GetValue(tokens)?.ToString();
                }
            }

            // format translation
            return Regex.Replace(text, @"{{([ \w\.\-]+)}}", match =>
            {
                string key = match.Groups[1].Value.Trim();
                return tokenLookup.TryGetValue(key, out string? value)
                ? (value ?? "")
                    : match.Value;
            });
        }
    }
}
