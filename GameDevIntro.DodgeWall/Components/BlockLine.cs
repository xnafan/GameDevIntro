using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleRandomDodge;
using System;

namespace GameDevIntro.DodgeWall.Components;

public class BlockLine : SimpleSprite
{

    #region Variables and Properties
    public int MissingBoxIndex { get; set; }
    public int NumberOfBoxes { get; set; }
    public SpriteBatch SpriteBatch { get; set; }
    #endregion

    public BlockLine(Vector2 position, Texture2D boxTexture) : base (position, boxTexture)
    {
        NumberOfBoxes = 16;
        SetRandomHole();
    }

    public void SetRandomHole()
    {
        MissingBoxIndex = Random.Shared.Next(NumberOfBoxes);
    }

    public void Update (GameTime gameTime)
    {
        Position += Vector2.UnitY * (float)gameTime.ElapsedGameTime.TotalMilliseconds * .2f;
    }

    public void Draw()
    {
        for (int i = 0; i < NumberOfBoxes; i++)
        {
            if (i != MissingBoxIndex)
            {
                DodgeWallGame.SpriteBatch.Draw(Texture, Position + i * Vector2.UnitX * Texture.Width, Color.LightBlue);
            }
        }
    }

    internal void MoveAboveScreenAndOpenARandomHole()
    {
        Position = Vector2.Zero - Vector2.UnitY * Texture.Height;
        SetRandomHole();
    }

    public bool Overlaps(Rectangle rect)
    {
        var testRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        for (int i = 0; i < NumberOfBoxes; i++)
        {
            testRectangle.X = i * Texture.Width;
            if (i != MissingBoxIndex && testRectangle.Intersects(rect))
            {
                return true;
            }
        }
        return false;
    }
}