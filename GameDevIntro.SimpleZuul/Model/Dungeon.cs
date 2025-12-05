using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevIntro.SimpleZuul.Model;
internal class Dungeon : IEnumerable<Tile>
{
    #region Properties
    private static Color[] _tileColors;
    private Texture2D _wallTiles, _playerTiles;
    public Tile[,] Tiles { get; set; }
    public Point PlayerPosition { get; set; }
    private bool _playerFacingLeft = true;

    public int Width { get => Tiles.GetLength(0); }
    public int Height { get => Tiles.GetLength(1); }

    #endregion
    public Dungeon(int width, int height, Texture2D wallTiles, Texture2D playerTiles)
    {
        Tiles = new Tile[width, height];
        _wallTiles = wallTiles;
        _playerTiles = playerTiles;
        GenerateTileColors();
    }

    private void GenerateTileColors()
    {
        //generate 8 colors from white to black
        int numShades = 2;
        _tileColors = new Color[numShades+1];
        for (int i = 0; i < numShades; i++)
        {
            int shade = 255 - (i * (255/numShades));
            _tileColors[i] = new Color(shade, shade, shade);
        }
        _tileColors[numShades] = Color.Black;
    }

    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(this.Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 topLeft)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var colorIndex = Math.Clamp((int)(Vector2.Distance(new Vector2(PlayerPosition.X, PlayerPosition.Y), new Vector2(x, y)) / 2), 0, _tileColors.Length - 1);
                Tiles[x, y].Draw(spriteBatch, gameTime,topLeft + new Vector2(x * _wallTiles.Height, y * _wallTiles.Height), _tileColors[colorIndex]);
            }
        }
        var sourceRect = new Rectangle(_playerTiles.Height * (gameTime.TotalGameTime.Milliseconds / 250 %2) + (!_playerFacingLeft ? _playerTiles.Width / 2 : 0), 0, _playerTiles.Height, _playerTiles.Height);

        spriteBatch.Draw(_playerTiles, new Rectangle((int)(topLeft.X + PlayerPosition.X * _wallTiles.Height), (int)(topLeft.Y + PlayerPosition.Y * _wallTiles.Height), _playerTiles.Height, _playerTiles.Height),
           sourceRect , Color.White);
    }

    public Tile.TileType MovePlayer(Point direction)
    {
        if (Tiles[PlayerPosition.X + direction.X, PlayerPosition.Y + direction.Y].Type == Tile.TileType.Wall)
        {
            return Tile.TileType.Empty;
        }

        PlayerPosition = new Point(
            PlayerPosition.X + direction.X,
            PlayerPosition.Y + direction.Y
        );
        if (direction.X < 0)
        {
            _playerFacingLeft = true;
        }
        else if (direction.X > 0)
        {
            _playerFacingLeft = false;
        }
        var tileType = Tiles[PlayerPosition.X, PlayerPosition.Y].Type;
        Tiles[PlayerPosition.X, PlayerPosition.Y].Type = Tile.TileType.Empty;
        return tileType;
    }
}