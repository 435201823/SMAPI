using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>An assembly load context which redirects assembly load requests if another mod already loaded a given assembly and marked it public.</summary>
    internal class ModAssemblyLoadContext : AssemblyLoadContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>The assembly load contexts for all loaded mods, including this one.</summary>
        private readonly IReadOnlyList<ModAssemblyLoadContext> ModAssemblyLoadContexts;

        /// <summary>A preprocessed list of private assembly names handled by this instance.</summary>
        public readonly HashSet<string> PrivateAssemblyNames;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod this context is for.</param>
        /// <param name="modAssemblyLoadContexts">The assembly load contexts for all loaded mods, including this one.</param>
        public ModAssemblyLoadContext(IModMetadata mod, IReadOnlyList<ModAssemblyLoadContext> modAssemblyLoadContexts)
            : base(mod.Manifest.UniqueID)
        {
            this.ModAssemblyLoadContexts = modAssemblyLoadContexts;
            this.PrivateAssemblyNames = new HashSet<string>(mod.Manifest.PrivateAssemblies.Select(p => p.Name));
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // use assembly loaded by another mod as long it's not private
            if (assemblyName.Name is not null)
            {
                foreach (ModAssemblyLoadContext context in this.ModAssemblyLoadContexts)
                {
                    if (object.ReferenceEquals(context, this))
                        continue;

                    foreach (Assembly assembly in context.Assemblies)
                    {
                        AssemblyName curName = assembly.GetName();
                        if (curName.Name is not null && !context.PrivateAssemblyNames.Contains(curName.Name) && curName == assemblyName)
                            return assembly;
                    }
                }
            }

            // fallback
            return base.Load(assemblyName);
        }
    }
}
