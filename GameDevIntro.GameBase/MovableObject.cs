using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDevIntro.GameBase;
public class MovableObject
{
    #region Properties

    /// <summary>
    /// Where the game object is located in the game world
    /// </summary>
    public Vector2 Position { get; set; } = new Vector2();

    /// <summary>
    /// What direction the game object is heading
    /// </summary>
    public Vector2 Direction { get; set; } = Vector2.Zero;

    /// <summary>
    /// The image to draw for the game object
    /// </summary>
    public Texture2D Texture { get; set; }

    /// <summary>
    /// The number of pixels to move each update
    /// </summary>
    public float Speed { get; set; }

    #endregion


    public MovableObject(Vector2 position, Texture2D texture, float speed = .25f, Vector2? direction = null)
    {
        Position = position;
        Direction = direction ?? Vector2.Zero; //if no direction was given, use no movement as default
        Texture = texture;
        Speed = speed;  
    }

    /// <summary>
    /// Updates the position of the game object by direction * speed
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
        //calculate how far up and to the left to position of the texture
        //to center it on the Position
        var quarterTextureSize = new Vector2(Texture.Width, Texture.Height) / 2;

        //draw the texture centered on the position
        spriteBatch.Draw(Texture, Position - quarterTextureSize, Color.White);
    }
}
