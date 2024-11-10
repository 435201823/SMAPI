using System;
using System.Threading.Tasks;
using Pathoschild.Http.Client;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport
{
    /// <inheritdoc cref="ICurseForgeExportApiClient" />
    public class CurseForgeExportApiClient : ICurseForgeExportApiClient
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="userAgent">The user agent for the CurseForge export API.</param>
        /// <param name="baseUrl">The base URL for the CurseForge export API.</param>
        public CurseForgeExportApiClient(string userAgent, string baseUrl)
        {
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <inheritdoc />
        public async Task<CurseForgeFullExport> FetchExportAsync()
        {
            IResponse response = await this.Client.GetAsync("");

            CurseForgeFullExport export = await response.As<CurseForgeFullExport>();
            export.LastModified = response.Message.Content.Headers.LastModified ?? throw new InvalidOperationException("Can't fetch from CurseForge export API: expected Last-Modified header wasn't set.");

            return export;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Client.Dispose();
        }
    }
}
