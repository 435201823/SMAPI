using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Buildings;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <c>BuildableGameLocation</c> methods to their newer form on <see cref="GameLocation"/> to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "RedundantBaseQualifier", Justification = SuppressReasons.BaseForClarity)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class BuildableGameLocationFacade : GameLocation, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public new bool buildStructure(Building b, Vector2 tileLocation, Farmer who, bool skipSafetyChecks = false)
        {
            return base.buildStructure(b, tileLocation, who, skipSafetyChecks);
        }

        public new bool destroyStructure(Vector2 tile)
        {
            return base.destroyStructure(tile);
        }

        public new bool destroyStructure(Building b)
        {
            return base.destroyStructure(b);
        }

        public new Building getBuildingAt(Vector2 tile)
        {
            return base.getBuildingAt(tile);
        }

        public new Building getBuildingByName(string name)
        {
            return base.getBuildingByName(name);
        }

        public Building? getBuildingUnderConstruction()
        {
            foreach (Building b in base.buildings)
            {
                if (b.daysOfConstructionLeft.Value > 0 || b.daysUntilUpgrade.Value > 0)
                    return b;
            }

            return null;
        }

        public int getNumberBuildingsConstructed(string name)
        {
            return base.getNumberBuildingsConstructed(name);
        }

        public bool isBuildable(Vector2 tileLocation)
        {
            return base.isBuildable(tileLocation);
        }

        public new bool isPath(Vector2 tileLocation)
        {
            return base.isPath(tileLocation);
        }

        public new bool isBuildingConstructed(string name)
        {
            return base.isBuildingConstructed(name);
        }

        public new bool isThereABuildingUnderConstruction()
        {
            return base.isThereABuildingUnderConstruction();
        }


        /*********
        ** Private methods
        *********/
        private BuildableGameLocationFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
