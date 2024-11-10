using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Caching.CurseForgeExport
{
    /// <summary>Manages cached mod data from the CurseForge export API in-memory.</summary>
    internal class CurseForgeExportCacheMemoryRepository : BaseCacheRepository, ICurseForgeExportCacheRepository
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cached mod data from the CurseForge export API.</summary>
        private CurseForgeFullExport? Data;

        /// <summary>When the data was last updated.</summary>
        private DateTimeOffset LastRefreshed;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool IsLoaded()
        {
            return this.Data?.Mods.Count > 0;
        }

        /// <inheritdoc />
        public DateTimeOffset? GetLastRefreshed()
        {
            return this.LastRefreshed;
        }

        /// <inheritdoc />
        public bool TryGetMod(uint id, [NotNullWhen(true)] out CurseForgeModExport? mod)
        {
            var data = this.Data?.Mods;

            if (data is null || !data.TryGetValue(id, out mod))
            {
                mod = null;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void SetData(CurseForgeFullExport? export)
        {
            this.Data = export;
            this.LastRefreshed = DateTimeOffset.UtcNow;
        }

        /// <inheritdoc />
        public bool IsStale(int staleMinutes)
        {
            return this.IsStale(this.LastRefreshed, staleMinutes);
        }
    }
}
