using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDevIntro.GameBase;
public class MovableObject
{
    public Vector2 Position{ get; set; } = new Vector2();
    public Vector2 Direction{ get; set; } = Vector2.Zero;
    public Texture2D  Texture { get; set; }
    public float Speed { get; set; } = 4;

    public MovableObject(Vector2 position, Vector2 direction, Texture2D texture)
    {
        Position = position;
        Direction = direction;
        Texture = texture;
    }

    public void Update(GameTime gameTime)
    {
        Position += Direction * Speed;
    }


    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(Texture, Position - new Vector2(Texture.Width, Texture.Height)/2, Color.White);
    }
}
