using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameDevIntro.SimpleZuul.Model;
internal class Tile
{
    Texture2D _spriteSheet;
    private static void GenerateTileColors()
    {
        //generate 8 colors from white to black
        _tileColors = new Color[8];
        for (int i = 0; i < 8; i++)
        {
            int shade = 255 - (i * 32);
            _tileColors[i] = new Color(shade, shade, shade);
        }
    }
    private static Color[] _tileColors;
    
    public enum TileType {Empty, Wall, Slime, Skeleton, Dragon, Chest }

    public TileType Type { get; set; }

    static Tile()
    {
        GenerateTileColors();
    }

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