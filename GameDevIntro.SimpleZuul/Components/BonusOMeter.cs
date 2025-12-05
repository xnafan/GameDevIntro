using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace GameDevIntro.SimpleZuul.Components;
internal class BonusOMeter : Sprite
{
    public int Score { get; private set; } = 0;
    public int MaxScore { get; private set; }
    private float _currentDisplayScore = 0;
    private Texture2D _filledTexture;
    public int CurrentBonus { get => (int)(((float)Score / (float)MaxScore) * 3); }
    public BonusOMeter(Vector2 position, Texture2D texture, GraphicsDevice graphics, int maxScore)
        : base(position, texture)
    {
        MaxScore = maxScore;
        _filledTexture = new Texture2D(graphics, 1, 1);
        _filledTexture.SetData(new[] { Color.White });
    }

    public void Update(int score, GameTime gameTime)
    {
        Score = score;
        //no movement for this sprite, so override the base Update to do nothing
        if (_currentDisplayScore < Score)
        {
            _currentDisplayScore += 1 + (Score - _currentDisplayScore) * 0.03f;
            if (_currentDisplayScore > Score)
                _currentDisplayScore = Score;
        }
    }

       public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        int maxHeight = 435;
        int fillTop = 100;
        float currentFillPercentage = Math.Clamp(_currentDisplayScore / MaxScore, 0, 1);
        float actualFillPercentage = Math.Clamp((float)Score / MaxScore, 0, 1);
        int currentFilledHeight = (int)(maxHeight * currentFillPercentage);
        int actualFilledHeight = (int)(maxHeight * actualFillPercentage);

        int fillWidth = 90;

        int fillBottom = (int)(Position.Y + fillTop + maxHeight);
        int fillLeft = (int)(Position.X + 40);
        
        var currentDestinationRect = new Rectangle(fillLeft, fillBottom - currentFilledHeight, fillWidth, currentFilledHeight);
        var actualDestinationRect = new Rectangle(fillLeft, fillBottom - actualFilledHeight, fillWidth, actualFilledHeight);

        spriteBatch.Draw(_filledTexture, actualDestinationRect, Color.LightGreen* 0.6f);
        spriteBatch.Draw(_filledTexture, currentDestinationRect,Color.Green);
        spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), Color.White);
    }

    internal void Reset()
    {
        Score = 0;
        _currentDisplayScore = 0;

    }
}