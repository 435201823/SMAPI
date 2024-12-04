using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>An assembly load context which contains all the assemblies loaded by a particular mod, and redirects requests for public assemblies already loaded by another mod.</summary>
    internal class ModAssemblyLoadContext : AssemblyLoadContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>A lookup of public assembly names to the load context which contains them.</summary>
        private static readonly Dictionary<string, ModAssemblyLoadContext> LoadContextsByPublicAssemblyName = new();

        /// <summary>The list of private assembly names handled by this instance.</summary>
        private readonly HashSet<string> PrivateAssemblyNames;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod this context is for.</param>
        public ModAssemblyLoadContext(IModMetadata mod)
            : base(mod.Manifest.UniqueID)
        {
            this.PrivateAssemblyNames = new HashSet<string>(mod.Manifest.PrivateAssemblies.Select(p => p.Name));
        }

        /// <summary>Cache an assembly added to this load context by SMAPI.</summary>
        /// <param name="assembly">The assembly that was loaded.</param>
        public void OnLoadedAssembly(Assembly assembly)
        {
            string? name = assembly.GetName().Name;

            if (name != null && !this.PrivateAssemblyNames.Contains(name))
                ModAssemblyLoadContext.LoadContextsByPublicAssemblyName.TryAdd(name, this);
        }

        /// <summary>Get whether an assembly is loaded and publicly available.</summary>
        /// <param name="assemblyName">The assembly name.</param>
        public bool IsLoadedPublicAssembly(string? assemblyName)
        {
            return assemblyName != null && ModAssemblyLoadContext.LoadContextsByPublicAssemblyName.ContainsKey(assemblyName);
        }

        /// <summary>Get whether an assembly is private to this context.</summary>
        /// <param name="assemblyName">The assembly name.</param>
        public bool IsPrivateAssembly(string? assemblyName)
        {
            return assemblyName != null && this.PrivateAssemblyNames.Contains(assemblyName);
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? name = assemblyName.Name;

            return name is not null && ModAssemblyLoadContext.LoadContextsByPublicAssemblyName.TryGetValue(name, out ModAssemblyLoadContext? otherContext) && otherContext.Name != this.Name
                ? otherContext.LoadFromAssemblyName(assemblyName)
                : base.Load(assemblyName);
        }
    }
}
