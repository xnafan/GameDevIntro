using GameDevIntro.SimpleZuul.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevIntro.SimpleZuul.Components;
internal class Dungeon : IEnumerable<Tile>
{
    #region Properties
    private static Color[] _tileColors;
    private Texture2D _wallTiles, _playerTiles;
    public Tile[,] Tiles { get; set; }
    public Player Player { get; set; }
    private bool _playerFacingLeft = true;

    public int Width { get => Tiles.GetLength(0); }
    public int Height { get => Tiles.GetLength(1); }
    public int ItemsLeft { get; set; }

    #endregion
    public Dungeon(int width, int height, Player player, Texture2D wallTiles, Texture2D playerTiles)
    {
        Tiles = new Tile[width, height];
        Player = player;
        _wallTiles = wallTiles;
        _playerTiles = playerTiles;
        GenerateTileColors();
    }

    private void GenerateTileColors()
    {
        //generate colors from white to black
        int numShades = 4;
        _tileColors = new Color[numShades+1];
        for (int i = 0; i < numShades; i++)
        {
            int shade = 255 - i * (255/numShades);
            _tileColors[i] = new Color(shade, shade, shade);
        }
        _tileColors[numShades] = Color.Black;
    }

    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 topLeft)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var colorIndex = Math.Clamp((int)(Vector2.Distance(new Vector2(Player.Position.X, Player.Position.Y), new Vector2(x, y)) / 2), 0, _tileColors.Length - 1);
                Tiles[x, y].Draw(spriteBatch, gameTime,topLeft + new Vector2(x * _wallTiles.Height, y * _wallTiles.Height), _tileColors[colorIndex]);
            }
        }
        if (Player.HitPoints > 0)
        {
            var sourceRect = new Rectangle(_playerTiles.Height * (gameTime.TotalGameTime.Milliseconds / 250 %2) + (!_playerFacingLeft ? _playerTiles.Width / 2 : 0), 0, _playerTiles.Height, _playerTiles.Height);

            var playerDrawPosition = new Vector2(topLeft.X + Player.Position.X * _wallTiles.Height, topLeft.Y + Player.Position.Y * _wallTiles.Height);
            spriteBatch.Draw(_playerTiles, new Rectangle((int)playerDrawPosition.X, (int)playerDrawPosition.Y, _playerTiles.Height, _playerTiles.Height),
               sourceRect , Color.White);

            var healthBarOffset = new Vector2(_wallTiles.Height / 2, -10);
            HealthBar.Draw(spriteBatch, playerDrawPosition + healthBarOffset, (float)Player.HitPoints / Player.MAX_HITPOINTS);
        }
    }

    public Tile.TileType MovePlayer(Point direction)
    {
        if (Tiles[Player.Position.X + direction.X, Player.Position.Y + direction.Y].Type == Tile.TileType.Wall)
        {
            return Tile.TileType.Empty;
        }

        Player.Position = new Point(
            Player.Position.X + direction.X,
            Player.Position.Y + direction.Y
        );
        if (direction.X < 0)
        {
            _playerFacingLeft = true;
        }
        else if (direction.X > 0)
        {
            _playerFacingLeft = false;
        }
        var tileType = Tiles[Player.Position.X, Player.Position.Y].Type;
        Tiles[Player.Position.X, Player.Position.Y].Type = Tile.TileType.Empty;
        return tileType;
    }
}