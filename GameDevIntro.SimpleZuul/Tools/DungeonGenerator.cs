using GameDevIntro.SimpleZuul.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameDevIntro.SimpleZuul.Tools;
internal static class DungeonGenerator
{
    private static readonly Point[] Directions = new[]
    {
        new Point(-2, 0),  // Left
        new Point(2, 0),   // Right
        new Point(0, -2),  // Up
        new Point(0, 2)    // Down
    };

    public static Dungeon GenerateDungeon(int width, int height, Texture2D spriteSheet, Texture2D playerSpriteSheet)
    {
        var dungeon = new Dungeon(width, height, spriteSheet, playerSpriteSheet);

        // Initialize all tiles as walls
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                dungeon.Tiles[x, y] = new Tile(Tile.TileType.Wall, spriteSheet);
            }
        }

        // If the dungeon is too small to carve, return as-is
        if (dungeon.Width < 5 || dungeon.Height < 5)
        {
            dungeon.PlayerPosition = new Point(width / 2, height / 2);
            return dungeon;
        }

        // Ensure odd dimensions for proper maze generation (adjust if even)
        int startX = (width / 2) | 1;  // Make odd
        int startY = (height / 2) | 1;  // Make odd
        
        // Clamp to valid inner bounds
        startX = Math.Clamp(startX, 1, width - 2);
        startY = Math.Clamp(startY, 1, height - 2);
        
        dungeon.PlayerPosition = new Point(startX, startY);

        // Breadth-first backtracking maze generation
        GenerateMazeUsingBreadthFirst(dungeon, startX, startY);

        // Poke random holes in walls to connect tunnels
        PokeRandomHoles(dungeon);

        AddContent(dungeon);

        return dungeon;
    }

    private static void AddContent(Dungeon dungeon)
    {
        //TODO: More sophisticated content placement
        // chests at the end of corridors
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                var deltaX = Math.Abs( x - dungeon.PlayerPosition.X);
                var deltaY = Math.Abs( y - dungeon.PlayerPosition.Y);
                if (deltaX < 2 || deltaY < 2)
                {
                    continue; // Skip player position
                }

                if (dungeon.Tiles[x, y].Type == Tile.TileType.Empty)
                {
                    // Simple random chance to add a monster
                    var rand = new Random();
                    int roll = rand.Next(100);
                    if (roll < 2) // 2% chance for Dragon
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Dragon;
                    }
                    else if (roll < 8) // 6% chance for Skeleton
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Skeleton;
                    }
                    else if (roll < 20) // 12% chance for Slime
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Slime;
                    }
                    else if (roll < 25) // 5% chance for Chest
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Chest;
                    }
                }
            }
        }
    }

    private static void GenerateMazeUsingBreadthFirst(Dungeon dungeon, int startX, int startY)
    {
        var random = new Random();
        var queue = new Queue<Point>();
        var visited = new bool[dungeon.Width, dungeon.Height];

        // Mark starting position as visited and carve it
        visited[startX, startY] = true;
        dungeon.Tiles[startX, startY].Type = Tile.TileType.Empty;

        // Add all four directions from starting point to queue
        var shuffledDirections = new List<Point>(Directions);
        ShuffleList(shuffledDirections, random);

        foreach (var direction in shuffledDirections)
        {
            var nextPoint = new Point(startX + direction.X, startY + direction.Y);
            if (IsValidCell(dungeon, nextPoint.X, nextPoint.Y) && !visited[nextPoint.X, nextPoint.Y])
            {
                queue.Enqueue(nextPoint);
            }
        }

        // Process queue until empty
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Skip if already visited
            if (visited[current.X, current.Y])
                continue;

            // Find unvisited neighbors that could connect to this cell
            var possibleConnections = new List<Point>();
            
            foreach (var direction in Directions)
            {
                var neighbor = new Point(current.X - direction.X, current.Y - direction.Y);
                if (IsValidCell(dungeon, neighbor.X, neighbor.Y) && visited[neighbor.X, neighbor.Y])
                {
                    possibleConnections.Add(neighbor);
                }
            }

            // If we can connect to at least one visited cell, carve this cell
            if (possibleConnections.Count > 0)
            {
                // Mark as visited and carve
                visited[current.X, current.Y] = true;
                dungeon.Tiles[current.X, current.Y].Type = Tile.TileType.Empty;

                // Choose a random connection and carve the path
                var connection = possibleConnections[random.Next(possibleConnections.Count)];
                var wallX = (current.X + connection.X) / 2;
                var wallY = (current.Y + connection.Y) / 2;
                dungeon.Tiles[wallX, wallY].Type = Tile.TileType.Empty;

                // Add unvisited neighbors to queue
                var newDirections = new List<Point>(Directions);
                ShuffleList(newDirections, random);

                foreach (var direction in newDirections)
                {
                    var nextPoint = new Point(current.X + direction.X, current.Y + direction.Y);
                    if (IsValidCell(dungeon, nextPoint.X, nextPoint.Y) && !visited[nextPoint.X, nextPoint.Y])
                    {
                        queue.Enqueue(nextPoint);
                    }
                }
            }
        }
    }

    private static void PokeRandomHoles(Dungeon dungeon)
    {
        var random = new Random();
        
        // Calculate number of holes based on dungeon size (roughly 10-20% of inner area)
        int innerArea = (dungeon.Width - 2) * (dungeon.Height - 2);
        int numHoles = Math.Max(2, innerArea / 20); // At least 2 holes, or 1 per 20 inner tiles
        
        var wallPositions = new List<Point>();
        
        // Find all wall positions (excluding outer walls) that are adjacent to empty spaces
        for (int x = 1; x < dungeon.Width - 1; x++)
        {
            for (int y = 1; y < dungeon.Height - 1; y++)
            {
                if (dungeon.Tiles[x, y].Type == Tile.TileType.Wall && IsWallAdjacentToEmpty(dungeon, x, y))
                {
                    wallPositions.Add(new Point(x, y));
                }
            }
        }
        
        // Randomly select and poke holes
        for (int i = 0; i < numHoles && wallPositions.Count > 0; i++)
        {
            int randomIndex = random.Next(wallPositions.Count);
            Point wallToRemove = wallPositions[randomIndex];
            
            // Poke the hole
            dungeon.Tiles[wallToRemove.X, wallToRemove.Y].Type = Tile.TileType.Empty;
            
            // Remove this position and nearby positions to avoid clustering holes too closely
            wallPositions.RemoveAt(randomIndex);
            RemoveNearbyPositions(wallPositions, wallToRemove, 2);
        }
    }

    private static bool IsWallAdjacentToEmpty(Dungeon dungeon, int x, int y)
    {
        // Check if the wall has at least one empty neighbor (creates connection opportunity)
        Point[] cardinalDirections = {
            new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)
        };
        
        int emptyNeighbors = 0;
        foreach (var dir in cardinalDirections)
        {
            int nx = x + dir.X;
            int ny = y + dir.Y;
            if (nx >= 0 && nx < dungeon.Width && ny >= 0 && ny < dungeon.Height)
            {
                if (dungeon.Tiles[nx, ny].Type == Tile.TileType.Empty)
                {
                    emptyNeighbors++;
                }
            }
        }
        
        // Only poke holes in walls that have exactly 2 empty neighbors on opposite sides
        // This creates meaningful connections without destroying the maze structure
        return emptyNeighbors >= 2;
    }

    private static void RemoveNearbyPositions(List<Point> positions, Point center, int radius)
    {
        positions.RemoveAll(pos => 
            Math.Abs(pos.X - center.X) <= radius && Math.Abs(pos.Y - center.Y) <= radius);
    }

    private static bool IsValidCell(Dungeon dungeon, int x, int y)
    {
        // Ensure we stay within bounds and maintain border walls
        return x > 0 && x < dungeon.Width - 1 && y > 0 && y < dungeon.Height - 1;
    }

    private static void ShuffleList<T>(List<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}