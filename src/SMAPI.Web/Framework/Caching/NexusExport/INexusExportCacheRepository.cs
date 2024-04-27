using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Caching.NexusExport
{
    /// <summary>Manages cached mod data from the Nexus export API.</summary>
    internal interface INexusExportCacheRepository : ICacheRepository
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get whether the export data is currently available.</summary>
        bool IsLoaded();

        /// <summary>Get when the export data was last fetched, or <c>null</c> if no data is currently available.</summary>
        DateTimeOffset? GetLastRefreshed();

        /// <summary>Get the cached data for a mod, if it exists in the export.</summary>
        /// <param name="id">The Nexus mod ID.</param>
        /// <param name="mod">The fetched metadata.</param>
        bool TryGetMod(uint id, [NotNullWhen(true)] out NexusModExport? mod);

        /// <summary>Set the cached data to use.</summary>
        /// <param name="export">The export received from the Nexus Mods API, or <c>null</c> to remove it.</param>
        void SetData(NexusFullExport? export);

        /// <summary>Get whether the cached data is stale.</summary>
        /// <param name="staleMinutes">The age in minutes before data is considered stale.</param>
        bool IsStale(int staleMinutes);
    }
}
