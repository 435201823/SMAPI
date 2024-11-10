using System.Threading.Tasks;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;

namespace StardewModdingAPI.Web.Framework.Clients.CurseForge
{
    /// <summary>A client for the CurseForge website which does nothing, used for local development.</summary>
    internal class DisabledCurseForgeExportApiClient : ICurseForgeExportApiClient
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public Task<CurseForgeFullExport> FetchExportAsync()
        {
            return Task.FromResult(
                new CurseForgeFullExport
                {
                    Mods = new()
                }
            );
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
