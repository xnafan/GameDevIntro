using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace GameDevIntro.GameBase.Model;

/// <summary>
/// A spirite is a movable object with a texture (image) to draw
/// </summary>
public class Sprite
{
    #region Properties

    /// <summary>
    /// Where the sprite is located in the game world
    /// </summary>
    public Vector2 Position { get; set; } = new Vector2();

    /// <summary>
    /// What direction the sprite is heading
    /// </summary>
    public Vector2 Direction { get; set; } = Vector2.Zero;

    /// <summary>
    /// The image to draw for the sprite
    /// </summary>
    public Texture2D Texture { get; private set; }

    /// <summary>
    /// The number of pixels to move each update
    /// </summary>
    public float Speed { get; set; }

    Vector2 _quarterSize;

    #endregion

    /// <summary>
    /// Constructor of a sprite object
    /// </summary>
    /// <param name="position">Where in the gameworld to place the sprite</param>
    /// <param name="texture">What image to use for drawing the object</param>
    /// <param name="speed">how many pixels to move at a time - modified by the elapsed GameTime</param>
    /// <param name="direction">The direction to move in</param>
    public Sprite(Vector2 position, Texture2D texture, float speed = .25f, Vector2? direction = null)
    {
        Position = position;
        Direction = direction ?? Vector2.Zero; //if no direction was given, use no movement as default
        Texture = texture;
        Speed = speed;  
        _quarterSize = new Vector2(Texture.Width / 2, Texture.Height / 2);
    }

    /// <summary>
    /// Updates the position of the sprite based on direction, speed, and elapsed game time since last update
    /// </summary>
    /// <param name="gameTime">the current gametime</param>
    public void Update(GameTime gameTime)
    {
        Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    /// <summary>
    /// Draws the objects texture (image) centered on the Position of the object
    /// </summary>
    /// <param name="spriteBatch">The spritebatch to draw to</param>
    /// <param name="gameTime">what the game time is</param>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        //uncomment the next line to add a shadow effect
        //spriteBatch.Draw(Texture, Position - quarterTextureSize + new Vector2(-5,5), Color.Black * 0.3f);

        //draw the texture centered on the position
        spriteBatch.Draw(Texture, Position - _quarterSize, Color.White);
    }

    public Rectangle GetBoundingRectangle()
    {
        return new Rectangle(
            (int)(Position.X - _quarterSize.X),
            (int)(Position.Y - _quarterSize.Y),
            Texture.Width,
            Texture.Height);
    }

}

