using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport;
using StardewModdingAPI.Toolkit.Framework.Clients.CurseForgeExport.ResponseModels;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport;
using StardewModdingAPI.Toolkit.Framework.Clients.NexusExport.ResponseModels;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;
using StardewModdingAPI.Web.Framework.Caching.CurseForgeExport;
using StardewModdingAPI.Web.Framework.Caching.Mods;
using StardewModdingAPI.Web.Framework.Caching.NexusExport;
using StardewModdingAPI.Web.Framework.Caching.Wiki;
using StardewModdingAPI.Web.Framework.Clients.CurseForge;
using StardewModdingAPI.Web.Framework.Clients.Nexus;
using StardewModdingAPI.Web.Framework.ConfigModels;

namespace StardewModdingAPI.Web
{
    /// <summary>A hosted service which runs background data updates.</summary>
    /// <remarks>Task methods need to be static, since otherwise Hangfire will try to serialize the entire instance.</remarks>
    internal class BackgroundService : IHostedService, IDisposable
    {
        /*********
        ** Fields
        *********/
        /// <summary>The background task server.</summary>
        private static BackgroundJobServer? JobServer;

        /// <summary>The cache in which to store wiki metadata.</summary>
        private static IWikiCacheRepository? WikiCache;

        /// <summary>The cache in which to store mod data.</summary>
        private static IModCacheRepository? ModCache;

        /// <summary>The HTTP client for fetching the mod export from the CurseForge export API.</summary>
        private static ICurseForgeExportApiClient? CurseForgeExportApiClient;

        /// <summary>The HTTP client for fetching the mod export from the CurseForge export API.</summary>
        private static ICurseForgeExportCacheRepository? CurseForgeExportCache;

        /// <summary>The cache in which to store mod data from the Nexus export API.</summary>
        private static INexusExportCacheRepository? NexusExportCache;

        /// <summary>The HTTP client for fetching the mod export from the Nexus Mods export API.</summary>
        private static INexusExportApiClient? NexusExportApiClient;

        /// <summary>The config settings for mod update checks.</summary>
        private static IOptions<ModUpdateCheckConfig>? UpdateCheckConfig;

        /// <summary>Whether the service has been started.</summary>
        [MemberNotNullWhen(true,
            nameof(BackgroundService.JobServer),
            nameof(BackgroundService.ModCache),
            nameof(BackgroundService.CurseForgeExportApiClient),
            nameof(BackgroundService.CurseForgeExportCache),
            nameof(BackgroundService.NexusExportApiClient),
            nameof(BackgroundService.NexusExportCache),
            nameof(BackgroundService.UpdateCheckConfig),
            nameof(BackgroundService.WikiCache)
        )]
        private static bool IsStarted { get; set; }

        /// <summary>The number of minutes a site export should be considered valid based on its last-updated date before it's ignored.</summary>
        private static int ExportStaleAge => (BackgroundService.UpdateCheckConfig?.Value.SuccessCacheMinutes ?? 0) + 10;


        /*********
        ** Public methods
        *********/
        /****
        ** Hosted service
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="wikiCache">The cache in which to store wiki metadata.</param>
        /// <param name="modCache">The cache in which to store mod data.</param>
        /// <param name="curseForgeExportCache">The cache in which to store mod data from the CurseForge export API.</param>
        /// <param name="curseForgeExportApiClient">The HTTP client for fetching the mod export from the CurseForge export API.</param>
        /// <param name="nexusExportCache">The cache in which to store mod data from the Nexus export API.</param>
        /// <param name="nexusExportApiClient">The HTTP client for fetching the mod export from the Nexus Mods export API.</param>
        /// <param name="hangfireStorage">The Hangfire storage implementation.</param>
        /// <param name="updateCheckConfig">The config settings for mod update checks.</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "The Hangfire reference forces it to initialize first, since it's needed by the background service.")]
        public BackgroundService(IWikiCacheRepository wikiCache, IModCacheRepository modCache, ICurseForgeExportCacheRepository curseForgeExportCache, ICurseForgeExportApiClient curseForgeExportApiClient, INexusExportCacheRepository nexusExportCache, INexusExportApiClient nexusExportApiClient, JobStorage hangfireStorage, IOptions<ModUpdateCheckConfig> updateCheckConfig)
        {
            BackgroundService.WikiCache = wikiCache;
            BackgroundService.ModCache = modCache;
            BackgroundService.CurseForgeExportApiClient = curseForgeExportApiClient;
            BackgroundService.CurseForgeExportCache = curseForgeExportCache;
            BackgroundService.NexusExportCache = nexusExportCache;
            BackgroundService.NexusExportApiClient = nexusExportApiClient;
            BackgroundService.UpdateCheckConfig = updateCheckConfig;

            _ = hangfireStorage; // parameter is only received to initialize it before the background service
        }

        /// <summary>Start the service.</summary>
        /// <param name="cancellationToken">Tracks whether the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.TryInit();

            bool enableCurseForgeExport = BackgroundService.CurseForgeExportApiClient is not DisabledCurseForgeExportApiClient;
            bool enableNexusExport = BackgroundService.NexusExportApiClient is not DisabledNexusExportApiClient;

            // set startup tasks
            BackgroundJob.Enqueue(() => BackgroundService.UpdateWikiAsync());
            if (enableCurseForgeExport)
                BackgroundJob.Enqueue(() => BackgroundService.UpdateCurseForgeExportAsync());
            if (enableNexusExport)
                BackgroundJob.Enqueue(() => BackgroundService.UpdateNexusExportAsync());
            BackgroundJob.Enqueue(() => BackgroundService.RemoveStaleModsAsync());

            // set recurring tasks
            RecurringJob.AddOrUpdate("update wiki data", () => BackgroundService.UpdateWikiAsync(), "*/10 * * * *");      // every 10 minutes
            if (enableCurseForgeExport)
                RecurringJob.AddOrUpdate("update CurseForge export", () => BackgroundService.UpdateCurseForgeExportAsync(), "*/10 * * * *");
            if (enableNexusExport)
                RecurringJob.AddOrUpdate("update Nexus export", () => BackgroundService.UpdateNexusExportAsync(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("remove stale mods", () => BackgroundService.RemoveStaleModsAsync(), "2/10 * * * *"); // offset by 2 minutes so it runs after updates (e.g. 00:02, 00:12, etc)

            BackgroundService.IsStarted = true;

            return Task.CompletedTask;
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Tracks whether the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            BackgroundService.IsStarted = false;

            if (BackgroundService.JobServer != null)
                await BackgroundService.JobServer.WaitForShutdownAsync(cancellationToken);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            BackgroundService.IsStarted = false;

            BackgroundService.JobServer?.Dispose();
        }

        /****
        ** Tasks
        ****/
        /// <summary>Update the cached wiki metadata.</summary>
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
        public static async Task UpdateWikiAsync()
        {
            if (!BackgroundService.IsStarted)
                throw new InvalidOperationException($"Must call {nameof(BackgroundService.StartAsync)} before scheduling tasks.");

            WikiModList wikiCompatList = await new ModToolkit().GetWikiCompatibilityListAsync();
            BackgroundService.WikiCache.SaveWikiData(wikiCompatList.StableVersion, wikiCompatList.BetaVersion, wikiCompatList.Mods);
        }

        /// <summary>Update the cached CurseForge mod dump.</summary>
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
        public static async Task UpdateCurseForgeExportAsync()
        {
            if (!BackgroundService.IsStarted)
                throw new InvalidOperationException($"Must call {nameof(BackgroundService.StartAsync)} before scheduling tasks.");

            CurseForgeFullExport data = await BackgroundService.CurseForgeExportApiClient.FetchExportAsync();

            var cache = BackgroundService.CurseForgeExportCache;
            cache.SetData(data);
            if (cache.IsStale(BackgroundService.ExportStaleAge))
                cache.SetData(null); // if the export is too old, fetch fresh mod data from the site/API instead
        }

        /// <summary>Update the cached Nexus mod dump.</summary>
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
        public static async Task UpdateNexusExportAsync()
        {
            if (!BackgroundService.IsStarted)
                throw new InvalidOperationException($"Must call {nameof(BackgroundService.StartAsync)} before scheduling tasks.");

            NexusFullExport data = await BackgroundService.NexusExportApiClient.FetchExportAsync();

            var cache = BackgroundService.NexusExportCache;
            cache.SetData(data);
            if (cache.IsStale(BackgroundService.ExportStaleAge))
                cache.SetData(null); // if the export is too old, fetch fresh mod data from the site/API instead
        }

        /// <summary>Remove mods which haven't been requested in over 48 hours.</summary>
        public static Task RemoveStaleModsAsync()
        {
            if (!BackgroundService.IsStarted)
                throw new InvalidOperationException($"Must call {nameof(BackgroundService.StartAsync)} before scheduling tasks.");

            // remove mods in mod cache
            BackgroundService.ModCache.RemoveStaleMods(TimeSpan.FromHours(48));

            // remove stale export cache
            if (BackgroundService.NexusExportCache.IsStale(BackgroundService.ExportStaleAge))
                BackgroundService.NexusExportCache.SetData(null);

            return Task.CompletedTask;
        }


        /*********
        ** Private method
        *********/
        /// <summary>Initialize the background service if it's not already initialized.</summary>
        /// <exception cref="InvalidOperationException">The background service is already initialized.</exception>
        private void TryInit()
        {
            if (BackgroundService.JobServer != null)
                throw new InvalidOperationException("The scheduler service is already started.");

            BackgroundService.JobServer = new BackgroundJobServer();
        }
    }
}
