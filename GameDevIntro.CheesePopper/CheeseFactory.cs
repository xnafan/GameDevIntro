using GameDevIntro.GameBase.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameDevIntro.CheesePopper;

/// <summary>
/// This class manages a collection of cheese sprites that move down the screen.
/// When a cheese sprite moves below the screen, it is repositioned to a random 
/// position above the screen.
/// </summary>
internal class CheeseFactory
{
    // A list to hold all the cheese sprites managed by this factory
    private readonly List<Sprite> cheeses = new();

    // The maximum number of cheese sprites allowed on screen
    public int MaxCheeses { get; set; }

    // The dimensions of the game screen
    public Rectangle ScreenDimensions { get; private set; }

    // The texture used for the cheese sprites
    public Texture2D CheeseTexture { get; private set; }

    // Spawn boundaries for cheese sprites
    int _minimumXSpawn, _maximumXSpawn, _minimumYSpawn, _maximumYSpawn ;

    public CheeseFactory(int maxCheeses, Rectangle screenDimensions, Texture2D cheeseTexture)
    {
        MaxCheeses = maxCheeses;
        ScreenDimensions = screenDimensions;
        CheeseTexture = cheeseTexture;
        _minimumXSpawn = CheeseTexture.Width;
        _maximumXSpawn = ScreenDimensions.Width - CheeseTexture.Width;
        _minimumYSpawn = -(ScreenDimensions.Height * 2) + CheeseTexture.Height;
        _maximumYSpawn = -CheeseTexture.Height;

        CreateCheesesInRandomPositionsAboveScreen();
    }

    private void CreateCheesesInRandomPositionsAboveScreen()
    {
        for (int i = 0; i < MaxCheeses; i++)
        {
            var cheese = new Sprite(

          GetRandomPositionAboveScreen(),
                    CheeseTexture,
                    .25f,
                    Vector2.UnitY);

            cheeses.Add(cheese);
        }
    }

    /// <summary>
    /// Returns a position vector with random X and Y coordinates above the screen
    /// with twice the screen height as the maximum height above.
    /// </summary>
    /// <returns></returns>
    private Vector2 GetRandomPositionAboveScreen()
    {
        return new Vector2(
                        Random.Shared.Next(_minimumXSpawn, _maximumXSpawn),
                        Random.Shared.Next(_minimumYSpawn, _maximumYSpawn)
                    );
    }

    /// <summary>
    /// Updates all cheese sprites, moving any that have moved below the screen
    /// to a new random position above the screen.
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        foreach (var sprite in cheeses)
        {
            sprite.Update(gameTime);
            if(sprite.Position.Y > ScreenDimensions.Height + CheeseTexture.Height)
            {
                sprite.Position = GetRandomPositionAboveScreen();
            }
        }
    }

    public Sprite CheckForCheeseAtPosition(Vector2 position)
    {
        foreach (var cheese in cheeses)
        {
            if (cheese.GetBoundingRectangle().Contains(position))
            {
                cheese.Position = GetRandomPositionAboveScreen();
                return cheese;
            }
        }
        return null;
    }

    public bool PopCheeseAtPosition(Vector2 position)
    {
        var cheese = CheckForCheeseAtPosition(position);
        if(cheese != null)
        {
            cheese.Position = GetRandomPositionAboveScreen();
        }
        return cheese != null;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        foreach (var cheese in cheeses)
        {
            cheese.Draw(spriteBatch, gameTime);
        }
    }
}
