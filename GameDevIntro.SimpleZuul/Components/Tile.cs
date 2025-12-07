using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameDevIntro.SimpleZuul.Model;
internal class Tile
{
    Texture2D _spriteSheet;
    
    public enum TileType {Empty, Wall, Slime, Skeleton, Dragon, Chest, Tombstone }

    public TileType Type { get; set; }

    public Tile(TileType type, Texture2D spriteSheet)
    {
        Type = type;
        _spriteSheet = spriteSheet;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 topLeft, Color? color)
    {
        color ??= Color.White;

        //draw tile based on type
        Rectangle sourceRectangle = new ((int)(Type) * _spriteSheet.Height, 0, _spriteSheet.Height, _spriteSheet.Height);
        spriteBatch.Draw(_spriteSheet, new Rectangle((int)topLeft.X, (int)topLeft.Y, _spriteSheet.Height, _spriteSheet.Height), sourceRectangle, color.Value);
    }
}