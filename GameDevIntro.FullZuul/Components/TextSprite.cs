using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDevIntro.SimpleZuul.Components;
internal class TextSprite
{

    private readonly Vector2 _shadowOffset = new Vector2(-3,3);
    public string Text { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector2  Direction { get; private set; }
    public Color Color { get; private set; }
    public float Speed { get; private set; }
    public float LifeTime { get; private set; }
    public bool IsExpired { get => LifeTime <= 0; }

    public SpriteFont Font { get; private set; }

    public TextSprite(string text, SpriteFont font, Vector2 position, Vector2 direction, Color color, float speed = 0.1f, float lifeTime = 1000f)
    {
        Text = text;
        Font = font;
        Position = position;
        Direction = direction;
        Color = color;
        Speed = speed;
        LifeTime = lifeTime;
    }

    public void Update(GameTime gameTime)
    {
        Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        LifeTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.DrawString(Font, Text, Position + _shadowOffset, Color.Black * .7f);
        spriteBatch.DrawString(Font, Text, Position, Color );
    }
}
