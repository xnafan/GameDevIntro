using GameDevIntro.CheesePopper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
namespace GameDevIntro.MousePopper;
public class MousePopperGame : Game
{
    #region Variables and enums
    public enum GameState { TitleScreen, Playing, GameOver }
    public GameState CurrentState { get; set; }

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _cheeseTexture, _logoTexture, _crosshairsTexture;
    private MouseState _currentMouseState, _previousMouseState;
    private SpriteFont _defaultFont;
    private CheeseFactory _cheeseFactory;
    #endregion

    #region Initialization
    public MousePopperGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        // Set resolution 
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _logoTexture = Content.Load<Texture2D>("graphics/cheesePopper_logo_small");
        _cheeseTexture = Content.Load<Texture2D>("graphics/cheese");
        _defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");
        _crosshairsTexture = Content.Load<Texture2D>("graphics/crosshairs");
        NewGame();
    }

    private void NewGame()
    {
      _cheeseFactory = new CheeseFactory(10, GraphicsDevice.Viewport.Bounds, _cheeseTexture);
    }

    #endregion

    #region Update and related
    protected override void Update(GameTime gameTime)
    {
        _cheeseFactory.Update(gameTime);
        var leftButtonClicked = false;
        _currentMouseState = Mouse.GetState();

        leftButtonClicked = (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released);
        var rightButtonClicked = (_currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released);

        if(rightButtonClicked)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if (leftButtonClicked)
                {
                    CurrentState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                if (leftButtonClicked)
                {
                    _cheeseFactory.PopCheeseAtPosition(new Vector2(_currentMouseState.X, _currentMouseState.Y));
                }
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


        _previousMouseState = _currentMouseState;
        base.Update(gameTime);
    }

    #endregion

    # region Update and related
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
    private void DrawTitleScreen(GameTime gameTime)
    {
        var centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;

        var bobbingOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 20);
        _spriteBatch.Draw(_logoTexture, new Vector2(centerOfScreen.X - _logoTexture.Width / 2, centerOfScreen.Y - _logoTexture.Height / 2 + (float)bobbingOffset), Color.White);

        _spriteBatch.DrawString(_defaultFont, "Click to begin game! Right click to toggle full screen", new Vector2(25, Window.ClientBounds.Height - 50), Color.Brown);
    } 
    
    private void DrawGameScreen(GameTime gameTime)
    {
        _cheeseFactory.Draw(_spriteBatch, gameTime);
        float mouseCursorScale = 1;
        if (_currentMouseState.LeftButton == ButtonState.Pressed)
        {
            mouseCursorScale = 0.8f;
        }

        //draw the crosshairs slightly smaller when the mouse button is down
        _spriteBatch.Draw(_crosshairsTexture, new Vector2(_currentMouseState.X - (_crosshairsTexture.Width * mouseCursorScale) / 2, _currentMouseState.Y - (_crosshairsTexture.Height * mouseCursorScale) / 2), null, Color.White, 0f, Vector2.Zero, mouseCursorScale, SpriteEffects.None, 0f);
    }
    #endregion
}