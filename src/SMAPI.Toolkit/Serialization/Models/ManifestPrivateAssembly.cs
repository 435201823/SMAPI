namespace StardewModdingAPI.Toolkit.Serialization.Models
{
    /// <inheritdoc cref="IManifestPrivateAssembly" />
    public class ManifestPrivateAssembly : IManifestPrivateAssembly
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool UsedDynamically { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="usedDynamically">Whether to disable warnings that an assembly appears to be unused, e.g. because it's accessed via reflection.</param>
        public ManifestPrivateAssembly(string name, bool usedDynamically)
        {
            this.Name = Manifest.NormalizeWhitespace(name);
            this.UsedDynamically = usedDynamically;
        }
    }
}
