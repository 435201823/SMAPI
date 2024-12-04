namespace StardewModdingAPI
{
    /// <summary>An assembly which is only loaded for the current mod. This won't be directly accessible by other mods, and will be ignored when another mod tries to use an assembly with the same name.</summary>
    public interface IManifestPrivateAssembly
    {
        /// <summary>The assembly name without the extension, like 'StardewModdingAPI'.</summary>
        public string Name { get; }

        /// <summary>Whether to disable warnings that an assembly appears to be unused, e.g. because it's accessed via reflection.</summary>
        public bool UsedDynamically { get; }
    }
}
