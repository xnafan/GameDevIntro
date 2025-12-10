using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleRandomDodge;
using System.Linq;

namespace GameDevIntro.DodgeWall.Components;

public class Player : SimpleSprite
{

    #region Properties and variables
    private float _speed = 6;
    #endregion


    public Player(Vector2 position, Texture2D texture) : base (position, texture) {}

    public void Update(GameTime gameTime)
    {
        var oldPosition = Position;
        Position += GetMovementFromKeyboard() * _speed * gameTime.ElapsedGameTime.Milliseconds/10;

        bool isWithinScreen = DodgeWallGame.ScreenBoundary.Contains(GetBounds());
        bool isCollidingWithLines = DodgeWallGame.BlockLines.Any(line => line.Overlaps(GetBounds()));

        if (!isWithinScreen || isCollidingWithLines)
        {
            Position = oldPosition;
        }
    }

    private Vector2 GetMovementFromKeyboard()
    {
        Vector2 movement = Vector2.Zero;
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Up)) { movement -= Vector2.UnitY; }
        if (keyboardState.IsKeyDown(Keys.Down)) { movement += Vector2.UnitY; }
        if (keyboardState.IsKeyDown(Keys.Right)) { movement += Vector2.UnitX ; }
        if (keyboardState.IsKeyDown(Keys.Left)) { movement -= Vector2.UnitX; }

        return movement;
    }

    public Rectangle GetBounds()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
    }

    public void Draw()
    {
        DodgeWallGame.SpriteBatch.Draw(Texture, Position, Color.Red);
    }
}