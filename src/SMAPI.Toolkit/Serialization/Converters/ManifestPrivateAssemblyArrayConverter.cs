using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Toolkit.Serialization.Models;

namespace StardewModdingAPI.Toolkit.Serialization.Converters
{
    /// <summary>Handles deserialization of <see cref="IManifestPrivateAssembly"/> arrays.</summary>
    internal class ManifestPrivateAssemblyArrayConverter : JsonConverter
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public override bool CanWrite => false;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ManifestPrivateAssembly[]);
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            List<ManifestPrivateAssembly> result = new List<ManifestPrivateAssembly>();

            foreach (JObject obj in JArray.Load(reader).Children<JObject>())
            {
                string name = obj.ValueIgnoreCase<string>(nameof(ManifestPrivateAssembly.Name))!; // will be validated separately if null
                bool usedDynamically = obj.ValueIgnoreCase<bool?>(nameof(ManifestPrivateAssembly.UsedDynamically)) ?? false;
                result.Add(new ManifestPrivateAssembly(name, usedDynamically));
            }

            return result.ToArray();
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("This converter does not write JSON.");
        }
    }
}
