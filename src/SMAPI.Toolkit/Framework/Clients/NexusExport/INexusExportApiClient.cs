using System;
using System.Threading.Tasks;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;

namespace StardewModdingAPI.Toolkit.Framework.Clients.NexusExport
{
    /// <summary>An HTTP client for fetching the mod export from the Nexus Mods export API.</summary>
    public interface INexusExportApiClient : IDisposable
    {
        /// <summary>Fetch the latest export file from the Nexus Mods export API.</summary>
        public Task<NexusFullExport> FetchExportAsync();
    }
}
