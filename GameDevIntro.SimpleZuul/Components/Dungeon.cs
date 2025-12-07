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
    public SpriteFont AttackBonusFont { get; set; }
    public Player Player { get; set; }
    private bool _playerFacingLeft = true;

    public int Width { get => Tiles.GetLength(0); }
    public int Height { get => Tiles.GetLength(1); }
    public int ItemsLeft { get; set; }

    #endregion
    public Dungeon(int width, int height, Player player, Texture2D wallTiles, Texture2D playerTiles, SpriteFont attackBonusFont)
    {
        Tiles = new Tile[width, height];
        Player = player;
        _wallTiles = wallTiles;
        _playerTiles = playerTiles;
        AttackBonusFont = attackBonusFont;
    }

    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tiles[x, y].Draw(spriteBatch, gameTime, new Vector2(x * _wallTiles.Height, y * _wallTiles.Height), Color.White);
            }
        }
        if (Player.HitPoints > 0)
        {
            DrawPlayer(spriteBatch, gameTime);
        }
    }

    private void DrawPlayer(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var sourceRect = new Rectangle(_playerTiles.Height * (gameTime.TotalGameTime.Milliseconds / 250 % 2) + (!_playerFacingLeft ? _playerTiles.Width / 2 : 0), 0, _playerTiles.Height, _playerTiles.Height);


        //calculate player draw position based on player position and wall tile size
        var playerDrawPosition = new Vector2(Player.Position.X * _wallTiles.Height, Player.Position.Y * _wallTiles.Height);

        spriteBatch.Draw(_playerTiles, playerDrawPosition,sourceRect, Color.White);

        var healthBarOffset = new Vector2(_wallTiles.Height / 2, -10);
        HealthBar.Draw(spriteBatch, playerDrawPosition + healthBarOffset, (float)Player.HitPoints / Player.MAX_HITPOINTS);

        if (Player.AttackRollBonus > 0)
        {
            //draw attack bonus bottom left corner of player with shadow

            var attackBonusText = $"+{Player.AttackRollBonus}";
            var textSize = AttackBonusFont.MeasureString(attackBonusText);
            var textOffset = Vector2.One * _wallTiles.Height * .8f;

            //draw an outline for better visibility 
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if(x == 0 && y == 0) continue;
                    spriteBatch.DrawString(AttackBonusFont, attackBonusText, playerDrawPosition + textOffset + new Vector2(x, y), Color.Black);
                }
            }

            spriteBatch.DrawString(AttackBonusFont, attackBonusText, playerDrawPosition + textOffset, Color.White); 
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