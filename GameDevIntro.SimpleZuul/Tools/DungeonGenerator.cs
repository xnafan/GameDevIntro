using GameDevIntro.SimpleZuul.Model;
using System;
using System.Drawing;

namespace GameDevIntro.SimpleZuul.Tools;
internal static class DungeonGenerator
{
    public static Dungeon GenerateDungeon(int width, int height)
    {
        var dungeon = new Dungeon(width, height); 

        // Initialize all tiles as walls
        SetAllTilesToBlocked(dungeon);

        // If the dungeon is too small to carve (need at least a3x3 to have an inner area), return as-is
        if (dungeon.Width <3 || dungeon.Height <3)
        {
            dungeon.PlayerPosition = new Point(width /2, height /2);
            return dungeon;
        }

        // Start player in the center and carve out from there
        var rng = new Random();
        int px = width /2;
        int py = height /2;
        dungeon.PlayerPosition = new Point(px, py);

        // Ensure starting tile is empty
        dungeon.Tiles[px, py].Type = Tile.TileType.Empty;
        int openCount =1;

        // Target number of open tiles (roughly a fraction of inner area)
        int innerArea = (dungeon.Width -2) * (dungeon.Height -2);
        int targetOpen = Math.Max(10, innerArea *40 /100); // at least10 tiles or ~40% of inner area

        while (openCount < targetOpen)
        {
            // Random walk: move one step in a random cardinal direction but never leave the inner area (1..width-2 /1..height-2)
            int dir = rng.Next(4);
            int nx = px;
            int ny = py;
            switch (dir)
            {
                case 0: nx = Math.Max(1, px -1); break; // left
                case 1: nx = Math.Min(dungeon.Width -2, px +1); break; // right
                case 2: ny = Math.Max(1, py -1); break; // up
                case 3: ny = Math.Min(dungeon.Height -2, py +1); break; // down
            }

            // Move
            px = nx; py = ny;

            // Carve current tile if it's a wall
            if (dungeon.Tiles[px, py].Type == Tile.TileType.Wall)
            {
                dungeon.Tiles[px, py].Type = Tile.TileType.Empty;
                openCount++;
            }

            // Occasionally carve an adjacent tile to create small open areas
            if (rng.NextDouble() <0.15)
            {
                int ax = px + (rng.Next(3) -1); // -1,0,1
                int ay = py + (rng.Next(3) -1);
                // keep inside inner area
                ax = Math.Clamp(ax,1, dungeon.Width -2);
                ay = Math.Clamp(ay,1, dungeon.Height -2);
                if (dungeon.Tiles[ax, ay].Type == Tile.TileType.Wall)
                {
                    dungeon.Tiles[ax, ay].Type = Tile.TileType.Empty;
                    openCount++;
                }
            }
        }

        return dungeon;
    }

    private static void SetAllTilesToBlocked(Dungeon dungeon)
    {
        foreach (var tile in dungeon.Tiles)
        {
            tile.Type = Tile.TileType.Wall;
        }
    }
}