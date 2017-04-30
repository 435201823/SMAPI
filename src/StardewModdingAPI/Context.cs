﻿using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Provides information about the current game state.</summary>
    internal static class Context
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether a player save has been loaded.</summary>
        public static bool IsSaveLoaded => Game1.hasLoadedGame && !string.IsNullOrEmpty(Game1.player.name);

        /// <summary>Whether the game is currently writing to the save file.</summary>
        public static bool IsSaving => SaveGame.IsProcessing;

        /// <summary>Whether the game is currently running the draw loop.</summary>
        public static bool IsInDrawLoop { get; set; }
    }
}
