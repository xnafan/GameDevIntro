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
    private Texture2D _wallTiles, _playerTiles;
    public Tile[,] Tiles { get; set; }
    public Player Player{ get; set; }
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
    }

    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 topLeft)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tiles[x, y].Draw(spriteBatch, gameTime,topLeft + new Vector2(x * _wallTiles.Height, y * _wallTiles.Height), Color.White);
            }
        }
        if(Player.HitPoints > 0)
        {

        var sourceRect = new Rectangle(_playerTiles.Height * (gameTime.TotalGameTime.Milliseconds / 250 %2) + (!_playerFacingLeft ? _playerTiles.Width / 2 : 0), 0, _playerTiles.Height, _playerTiles.Height);

        spriteBatch.Draw(_playerTiles, new Rectangle((int)(topLeft.X + Player.Position.X * _wallTiles.Height), (int)(topLeft.Y + Player.Position.Y * _wallTiles.Height), _playerTiles.Height, _playerTiles.Height),
           sourceRect , Color.White);

            HealthBar.Draw(spriteBatch, new Vector2(topLeft.X + Player.Position.X * _wallTiles.Height + _playerTiles.Height / 2, topLeft.Y + Player.Position.Y * _wallTiles.Height - 10),
                (float)Player.HitPoints / Player.MAX_HITPOINTS);
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