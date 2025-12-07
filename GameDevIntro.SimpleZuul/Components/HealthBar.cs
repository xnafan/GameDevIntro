using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDevIntro.SimpleZuul.Components;

//a class to represent a health bar component
internal class HealthBar
{
    private static Texture2D _whiteTexture;
    public  SpriteBatch SpriteBatch { get; set; }

    private static Color[] _colors = [Color.Red, Color.Red, Color.Orange, Color.Orange, Color.Yellow, Color.Yellow, Color.Yellow, Color.Green, Color.Green, Color.Green];

    public static void Draw(SpriteBatch spriteBatch, Vector2 topCenter, float percentage, int width = 40, int height = 8, int borderThickness = 1)
    {
        if (_whiteTexture == null)
        {
            _whiteTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White }); 
        }

        // Draw border
        spriteBatch.Draw(_whiteTexture, new Rectangle((int)(topCenter.X - width / 2 - borderThickness), (int)(topCenter.Y - borderThickness), width + borderThickness * 2, height + borderThickness * 2), Color.Gray);
        
        // Draw health bar
        spriteBatch.Draw(_whiteTexture, new Rectangle((int)(topCenter.X - width / 2), (int)(topCenter.Y), width, height), Color.Black);

        // Draw filled portion (for example, 70% health)
        // ensure index is between 0 and 9
        int colorIndex = (int) MathHelper.Clamp((percentage * 10) -1, 0,9);

        spriteBatch.Draw(_whiteTexture, new Rectangle((int)(topCenter.X - width / 2), (int)(topCenter.Y), (int)(width * percentage), height), _colors[colorIndex]);

    }
}