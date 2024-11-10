using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels
{
    /// <summary>The metadata for all Stardew Valley from the CurseForge export API.</summary>
    public class CurseForgeFullExport
    {
        /// <summary>The mod data indexed by public mod ID.</summary>
        public Dictionary<uint, CurseForgeModExport> Mods { get; set; } = new();

        /// <summary>When the data was last updated.</summary>
        public DateTimeOffset LastModified { get; set; }
    }
}
