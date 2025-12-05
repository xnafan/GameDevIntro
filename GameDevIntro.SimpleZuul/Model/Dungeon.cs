using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace GameDevIntro.SimpleZuul.Model;
internal class Dungeon : IEnumerable<Tile>
{
    #region Properties

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
    }

    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(this.Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 topLeft)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tiles[x, y].Draw(spriteBatch, gameTime,topLeft + new Vector2(x * _wallTiles.Height, y * _wallTiles.Height));
            }
        }
        var sourceRect = new Rectangle(_playerTiles.Height * (gameTime.TotalGameTime.Milliseconds / 250 %2) + (!_playerFacingLeft ? _playerTiles.Width / 2 : 0), 0, _playerTiles.Height, _playerTiles.Height);

        spriteBatch.Draw(_playerTiles, new Rectangle((int)(topLeft.X + PlayerPosition.X * _wallTiles.Height), (int)(topLeft.Y + PlayerPosition.Y * _wallTiles.Height), _playerTiles.Height, _playerTiles.Height),
           sourceRect , Color.White);
    }

    public void MovePlayer(int xMovement, int yMovement)
    {
        if (Tiles[PlayerPosition.X + xMovement, PlayerPosition.Y + yMovement].Type == Tile.TileType.Wall)
        {
            return;
        }

        PlayerPosition = new Point(
            PlayerPosition.X + xMovement,
            PlayerPosition.Y + yMovement
        );
        if (xMovement < 0)
        {
            _playerFacingLeft = true;
        }
        else if (xMovement > 0)
        {
            _playerFacingLeft = false;
        }

        Tiles[PlayerPosition.X, PlayerPosition.Y].Type = Tile.TileType.Empty;
    }
}