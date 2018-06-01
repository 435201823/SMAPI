using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="IWorldEvents.LocationsChanged"/> event.</summary>
    public class WorldLocationsChangedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The added locations.</summary>
        public IEnumerable<GameLocation> Added { get; }

        /// <summary>The removed locations.</summary>
        public IEnumerable<GameLocation> Removed { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="added">The added locations.</param>
        /// <param name="removed">The removed locations.</param>
        public WorldLocationsChangedEventArgs(IEnumerable<GameLocation> added, IEnumerable<GameLocation> removed)
        {
            this.Added = added.ToArray();
            this.Removed = removed.ToArray();
        }
    }
}
