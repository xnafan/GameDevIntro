using GameDevIntro.GameBase.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDevIntro.CheesePopper;

/// <summary>
/// This class manages a collection of cheese sprites that move down the screen.
/// When a cheese sprite moves below the screen, it is repositioned to a random 
/// position above the screen.
/// </summary>
public class CheeseFactory
{
    // A list to hold all the cheese sprites managed by this factory
    private readonly List<Sprite> _cheeses = new();

    // The maximum number of cheese sprites allowed on screen
    public int MaxCheeses { get; set; }

    // The dimensions of the game screen
    public Rectangle ScreenDimensions { get; private set; }

    // The texture used for the cheese sprites
    public Texture2D CheeseTexture { get; private set; }

    public int CheesesMissed { get; private set; }

    // Spawn boundaries for cheese sprites
    private readonly int _minimumXSpawn, _maximumXSpawn, _maximumYSpawn;
    private int _minimumYSpawn;
    private readonly SoundEffect _failEffect;
    
    // Cache for screen boundary checking
    private readonly float _screenBottomBoundary;

    public CheeseFactory(int maxCheeses, Rectangle screenDimensions, Texture2D cheeseTexture, SoundEffect failEffect)
    {
        MaxCheeses = maxCheeses;
        ScreenDimensions = screenDimensions;
        CheeseTexture = cheeseTexture;
        _minimumXSpawn = CheeseTexture?.Width ?? 32;
        _maximumXSpawn = ScreenDimensions.Width - (CheeseTexture?.Width ?? 32);
        _minimumYSpawn = -(ScreenDimensions.Height * 2) + (CheeseTexture?.Height ?? 32);
        _maximumYSpawn = -(CheeseTexture?.Height ?? 32);

        _failEffect = failEffect;
        
        // Cache screen boundary for performance
        _screenBottomBoundary = ScreenDimensions.Height + (CheeseTexture?.Height / 2 ?? 16);

        CreateCheesesInRandomPositionsAboveScreen();
    }

    private void CreateCheesesInRandomPositionsAboveScreen()
    {
        for (int i = 0; i < MaxCheeses; i++)
        {
            var cheese = new Sprite(
                GetRandomPositionAboveScreen(),
                CheeseTexture,
                .2f,
                Vector2.UnitY);
                
            _cheeses.Add(cheese);
        }
        SortCheeses();
    }

    private void SortCheeses()
    {
        // Sort cheeses by Y position descending for proper drawing order
        _cheeses.Sort((a, b) => b.Position.Y.CompareTo(a.Position.Y));
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
        // Use for loop instead of foreach for better performance
        for (int i = 0; i < _cheeses.Count; i++)
        {
            var sprite = _cheeses[i];
            sprite.Update(gameTime);
            
            if (sprite.Position.Y > _screenBottomBoundary)
            {
                CheesesMissed++;
                _failEffect?.Play();
                sprite.Position = GetRandomPositionAboveScreen();
                SortCheeses();
            }
        }
    }

    /// <summary>
    /// Returns the cheese sprite at the given position, if any.
    /// Early exit optimization for better performance.
    /// </summary>
    /// <param name="position">Which position to check at</param>
    /// <returns></returns>
    public Sprite CheckForCheeseAtPosition(Vector2 position)
    {
        // Use for loop for better performance and early exit
        for (int i = 0; i < _cheeses.Count; i++)
        {
            var cheese = _cheeses[i];
            if (cheese.GetBoundingRectangle().Contains(position))
            {
                Debug.WriteLine("Cheese popped at position: " + position);
                return cheese;
            }
        }
        return null;
    }

    public bool PopCheeseAtPosition(Vector2 position)
    {
        var cheese = CheckForCheeseAtPosition(position);
        if (cheese != null)
        {
            cheese.Position = GetRandomPositionAboveScreen();
            SortCheeses();

            // Gradually increase minimum Y spawn to make game harder
            // because cheeses will spawn closer above the screen
            _minimumYSpawn += 5;
            Debug.Write(_minimumYSpawn);
            // Cap the minimum Y spawn to avoid making it too hard
            _minimumYSpawn = Math.Min(_minimumYSpawn, -150);
            return true;
        }
        return false;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // reverse for loop to draw from back to front
        // This ensures that cheeses lower on the screen are drawn over those higher up
        for (int i = _cheeses.Count-1; i >= 0 ; i--)
        {
            _cheeses[i].Draw(spriteBatch, gameTime);
        }
    }
}
