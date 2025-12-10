 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleRandomDodge
{
    public class SimpleSprite
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }

        public SimpleSprite(Vector2 position, Texture2D boxTexture)
        {
            Position = position;
            Texture = boxTexture;
        }
    }
}