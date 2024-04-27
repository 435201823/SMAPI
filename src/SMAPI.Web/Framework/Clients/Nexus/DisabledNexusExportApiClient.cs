using System;
using System.Threading.Tasks;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Clients.Nexus
{
    /// <summary>A client for the Nexus website which does nothing, used for local development.</summary>
    internal class DisabledNexusExportApiClient : INexusExportApiClient
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public Task<NexusFullExport> FetchExportAsync()
        {
            return Task.FromResult(
                new NexusFullExport
                {
                    Data = new(),
                    LastUpdated = DateTimeOffset.UtcNow
                }
            );
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
