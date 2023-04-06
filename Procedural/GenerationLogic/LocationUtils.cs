using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural.GenerationLogic
{
    static class LocationUtils
    {
        /// <summary>
        /// Gets a random tile from this location but make sure that it is one of the available tiles and updates availableTileIds list
        /// </summary>
        public static StoryLocation.TileInfo GetRandomTileInfo(SimpleGeneratorContext ctx, StoryLocation location)
        {
            // Determine which ids are actually valid
            var validTiles = location.KnownTiles.Values
                .Where(t => ctx.Scenario.filteredGlobalTilePool.Contains(t.IdNumber))
                .ToList();
            if (validTiles.Count == 0)
            {
                ctx.GeneratorWarnings.Add("GetRandomTileInfo(): Couldn not find available '" + location.Name + "' tile anymore!");
                return null;
            }

            // Then take one at random
            var tile = validTiles.GetRandomFromEnumerable(ctx.Random);
            ctx.Scenario.filteredGlobalTilePool.Remove(tile.IdNumber);
            return tile;
        }

        /// <summary>
        /// Creates a new tile and adds it to the given Chapter 
        /// </summary>
        public static BaseTile CreateRandomTileAndAddtoTileset(SimpleGeneratorContext ctx, Chapter tileset, StoryLocation primaryLocation, IEnumerable<StoryLocation> secondaryLocations, bool mustBeFromPrimary = false)
        {
            // Gather tile info
            var tileInfo = GetRandomTileInfo(ctx, primaryLocation);
            if (tileInfo == null)
            {
                // Primary location did not yield valid tile
                if (mustBeFromPrimary)
                {
                    throw new Exception("Primary location did not contain valid tile!");
                }

                // Test secondaries
                if (secondaryLocations != null)
                {
                    foreach (var secondaryLocation in secondaryLocations)
                    {
                        tileInfo = GetRandomTileInfo(ctx, secondaryLocation);
                        if (tileInfo != null)
                        {
                            break;
                        }
                    }
                }
                if (tileInfo == null)
                {
                    throw new Exception("Primary or Secondary locations did not contain valid tile!");
                }
            }

            // Create the tile itself and add the exploration token
            // TODO: what does skipBuild mean? if false then we get some coordinate exception, might be just for the "Start" chapter since others use random tiles
            var tile = new HexTile(tileInfo.IdNumber, skipBuild: true)
            {
                tileSide = tileInfo.TileSide,
                flavorBookData = new TextBookData()
                {
                    pages = new List<string>() // <-- This needs to always exist at this level to avoid NULL ref!
                }
            };
            if (tileInfo.ExplorationTexts?.Count > 0)
            {
                tile.flavorBookData.pages.Add(tileInfo.ExplorationTexts.GetRandomFromEnumerable(ctx.Random));
            }
            else
            {
                tile.flavorBookData.pages.Add("");             
            }

            // Add the chapter and return created tile
            tileset.AddTile(tile);
            return tile;
        }
    }
}
