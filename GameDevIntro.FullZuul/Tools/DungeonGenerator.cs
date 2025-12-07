using GameDevIntro.SimpleZuul.Components;
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

    // Cardinal directions for immediate player area excavation
    private static readonly Point[] CardinalDirections = new[]
    {
        new Point(-1, 0),  // Left
        new Point(1, 0),   // Right
        new Point(0, -1),  // Up
        new Point(0, 1)    // Down
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

        // Random depth-first maze generation
        GenerateRandomMaze(dungeon, startX, startY);

        // Poke random holes in walls to connect tunnels
        PokeRandomHoles(dungeon);

        AddContent(dungeon);

        return dungeon;
    }

    private static void GenerateRandomMaze(Dungeon dungeon, int startX, int startY)
    {
        var random = new Random();
        var stack = new Stack<Point>();
        var visited = new bool[dungeon.Width, dungeon.Height];

        // Mark starting position as visited and carve it
        visited[startX, startY] = true;
        dungeon.Tiles[startX, startY].Type = Tile.TileType.Empty;

        // Excavate the four cardinal direction tiles from player position to ensure mobility
        foreach (var direction in CardinalDirections)
        {
            int adjacentX = startX + direction.X;
            int adjacentY = startY + direction.Y;
            
            // Only excavate if within bounds (not touching outer walls)
            if (adjacentX > 0 && adjacentX < dungeon.Width - 1 && 
                adjacentY > 0 && adjacentY < dungeon.Height - 1)
            {
                dungeon.Tiles[adjacentX, adjacentY].Type = Tile.TileType.Empty;
                visited[adjacentX, adjacentY] = true;
            }
        }

        // Start with the initial position
        stack.Push(new Point(startX, startY));

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            
            // Get all unvisited neighbors
            var unvisitedNeighbors = new List<Point>();
            
            foreach (var direction in Directions)
            {
                var neighbor = new Point(current.X + direction.X, current.Y + direction.Y);
                if (IsValidCell(dungeon, neighbor.X, neighbor.Y) && !visited[neighbor.X, neighbor.Y])
                {
                    unvisitedNeighbors.Add(neighbor);
                }
            }

            if (unvisitedNeighbors.Count > 0)
            {
                // Randomly choose one unvisited neighbor
                var chosenNeighbor = unvisitedNeighbors[random.Next(unvisitedNeighbors.Count)];
                
                // Mark as visited and carve
                visited[chosenNeighbor.X, chosenNeighbor.Y] = true;
                dungeon.Tiles[chosenNeighbor.X, chosenNeighbor.Y].Type = Tile.TileType.Empty;
                
                // Carve the wall between current and chosen neighbor
                var wallX = (current.X + chosenNeighbor.X) / 2;
                var wallY = (current.Y + chosenNeighbor.Y) / 2;
                dungeon.Tiles[wallX, wallY].Type = Tile.TileType.Empty;
                
                // Push the chosen neighbor onto the stack
                stack.Push(chosenNeighbor);
            }
            else
            {
                // No unvisited neighbors, backtrack
                stack.Pop();
            }
        }

        // Ensure all reachable areas are carved (handle disconnected regions)
        for (int attempts = 0; attempts < 5; attempts++)
        {
            bool foundUnvisited = false;
            
            for (int x = 1; x < dungeon.Width - 1; x += 2)
            {
                for (int y = 1; y < dungeon.Height - 1; y += 2)
                {
                    if (!visited[x, y])
                    {
                        // Found an unvisited cell, start a new branch from here
                        visited[x, y] = true;
                        dungeon.Tiles[x, y].Type = Tile.TileType.Empty;
                        
                        // Try to connect to an existing path
                        var nearbyVisited = new List<Point>();
                        foreach (var direction in Directions)
                        {
                            var neighbor = new Point(x + direction.X, y + direction.Y);
                            if (IsValidCell(dungeon, neighbor.X, neighbor.Y) && visited[neighbor.X, neighbor.Y])
                            {
                                nearbyVisited.Add(neighbor);
                            }
                        }
                        
                        if (nearbyVisited.Count > 0)
                        {
                            // Connect to a random nearby visited cell
                            var connection = nearbyVisited[random.Next(nearbyVisited.Count)];
                            var wallX = (x + connection.X) / 2;
                            var wallY = (y + connection.Y) / 2;
                            dungeon.Tiles[wallX, wallY].Type = Tile.TileType.Empty;
                        }
                        
                        // Continue maze generation from this new point
                        stack.Push(new Point(x, y));
                        while (stack.Count > 0)
                        {
                            var current = stack.Peek();
                            var unvisitedNeighbors = new List<Point>();
                            
                            foreach (var direction in Directions)
                            {
                                var neighbor = new Point(current.X + direction.X, current.Y + direction.Y);
                                if (IsValidCell(dungeon, neighbor.X, neighbor.Y) && !visited[neighbor.X, neighbor.Y])
                                {
                                    unvisitedNeighbors.Add(neighbor);
                                }
                            }

                            if (unvisitedNeighbors.Count > 0)
                            {
                                var chosenNeighbor = unvisitedNeighbors[random.Next(unvisitedNeighbors.Count)];
                                visited[chosenNeighbor.X, chosenNeighbor.Y] = true;
                                dungeon.Tiles[chosenNeighbor.X, chosenNeighbor.Y].Type = Tile.TileType.Empty;
                                
                                var wallX = (current.X + chosenNeighbor.X) / 2;
                                var wallY = (current.Y + chosenNeighbor.Y) / 2;
                                dungeon.Tiles[wallX, wallY].Type = Tile.TileType.Empty;
                                
                                stack.Push(chosenNeighbor);
                            }
                            else
                            {
                                stack.Pop();
                            }
                        }
                        
                        foundUnvisited = true;
                        break;
                    }
                }
                if (foundUnvisited) break;
            }
            
            if (!foundUnvisited) break; // No more unvisited cells
        }
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
                        dungeon.ItemsLeft++;
                    }
                    else if (roll < 8) // 6% chance for Skeleton
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Skeleton;
                        dungeon.ItemsLeft++;
                    }
                    else if (roll < 20) // 12% chance for Slime
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Slime;
                        dungeon.ItemsLeft++;
                    }
                    else if (roll < 25) // 5% chance for Chest
                    {
                        dungeon.Tiles[x, y].Type = Tile.TileType.Chest;
                        dungeon.ItemsLeft++;
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