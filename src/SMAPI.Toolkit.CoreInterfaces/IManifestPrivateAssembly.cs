namespace StardewModdingAPI
{
    /// <summary>An assembly which should only be referenced by the current mod. It will be ignored when another mod tries to use an assembly with the same name.</summary>
    public interface IManifestPrivateAssembly
    {
        /// <summary>The assembly name without metadata, like 'Newtonsoft.Json'.</summary>
        public string Name { get; }

        /// <summary>Whether to disable warnings that an assembly seems to be unused, e.g. because it's accessed via reflection.</summary>
        public bool UsedDynamically { get; }
    }
}
