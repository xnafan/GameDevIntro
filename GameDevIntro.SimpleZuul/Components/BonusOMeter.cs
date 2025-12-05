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
            _currentDisplayScore += 1 + (Score - _currentDisplayScore) * 0.1f;
            if (_currentDisplayScore > Score)
                _currentDisplayScore = Score;
        }
    }

       public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        int maxHeight = 435;
        int fillTop = 100;
        float fillPercentage = Math.Clamp(_currentDisplayScore / MaxScore, 0, 1);
        int filledHeight = (int)(maxHeight * fillPercentage);
        int fillWidth = 90;

        int fillBottom = (int)(Position.Y + fillTop + maxHeight);
        int fillLeft = (int)(Position.X + 40);
        var destinationRect = new Rectangle(fillLeft, fillBottom - filledHeight, fillWidth, filledHeight);

        spriteBatch.Draw(_filledTexture, destinationRect,Color.Green);
        spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), Color.White);
    }
}