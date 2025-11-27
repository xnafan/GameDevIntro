using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameDevIntro.MousePopper;
public class MousePopperGame : Game
{
    public enum GameState{TitleScreen, Playing, GameOver}
    public GameState CurrentState { get; set; }

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _cheeseTexture, _logoTexture;
    private MouseState _currentMouseState, _previousMouseState;
    private SpriteFont _defaultFont;

    public MousePopperGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        // Set resolution 
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _logoTexture = Content.Load<Texture2D>("graphics/cheesePopper_logo_small");
        _cheeseTexture = Content.Load<Texture2D>("graphics/cheese_512px");
        _defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");

    }

    protected override void Update(GameTime gameTime)
    {
        var leftButtonClicked = false;
        _currentMouseState = Mouse.GetState();

        leftButtonClicked = (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released);


        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if(leftButtonClicked)
                {
                    CurrentState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                if (leftButtonClicked)
                {
                    CurrentState = GameState.TitleScreen;
                }
                break;
            default:
                break;
        }



        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        GraphicsDevice.Clear(Color.Wheat);

        _spriteBatch.Begin();

        switch (CurrentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen(gameTime);

                break;
            case GameState.Playing:
                DrawGameScreen(gameTime);
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }

        // Draw game elements here
      
        _spriteBatch.End();


    }

    private void DrawGameScreen(GameTime gameTime)
    {
        _spriteBatch.Draw(_cheeseTexture,new Vector2(_currentMouseState.X, _currentMouseState.Y), Color.White);
    }

    private void DrawTitleScreen(GameTime gameTime)
    {
        var centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;

        var bobbingOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 20);
        _spriteBatch.Draw(_logoTexture, new Vector2(centerOfScreen.X - _logoTexture.Width / 2, centerOfScreen.Y - _logoTexture.Height / 2 + (float)bobbingOffset), Color.White);

        _spriteBatch.DrawString(_defaultFont, "Click to begin game!", new Vector2(25, Window.ClientBounds.Height - 50), Color.Brown);
    }
}
