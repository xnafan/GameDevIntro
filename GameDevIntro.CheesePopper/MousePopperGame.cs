using GameDevIntro.CheesePopper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
namespace GameDevIntro.MousePopper;
public class MousePopperGame : Game
{
    #region Variables and enums
    public enum GameState { TitleScreen, Playing, GameOver }
    public GameState CurrentState { get; set; }

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _cheeseTexture, _logoTexture, _crosshairsTexture, _heartTexture, _gameOverTexture, _pointsTexture;
    private Song _backgroundMusic;
    private SoundEffect _popSoundEffect, _failEffect;
    private MouseState _currentMouseState, _previousMouseState;
    private SpriteFont _defaultFont;
    private CheeseFactory _cheeseFactory;
    private int _score;
    private const int MAX_LIVES = 5;
    private int _livesLeft;
    private bool _rightButtonClicked, _leftButtonClicked;
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
        _heartTexture = Content.Load<Texture2D>("graphics/heart");
        _gameOverTexture = Content.Load<Texture2D>("graphics/gameover");
        _pointsTexture = Content.Load<Texture2D>("graphics/points_128px");
        
        _popSoundEffect = Content.Load<SoundEffect>("Sounds/pop");
        _failEffect = Content.Load<SoundEffect>("Sounds/fail");

        _backgroundMusic = Content.Load<Song>("Sounds/music");

        NewGame();
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_backgroundMusic);
    }

    private void NewGame()
    {
        _cheeseFactory = new CheeseFactory(10, GraphicsDevice.Viewport.Bounds, _cheeseTexture, _failEffect);
        _score = 0;
        _livesLeft = MAX_LIVES;
    }

    #endregion

    #region Update and related
    protected override void Update(GameTime gameTime)
    {
        UpdateButtonStates();

        ToggleFullScreenIfRightButtonClicked();
        ExitGameIfEscapeKeyPressed();


        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if (_leftButtonClicked)
                {
                    CurrentState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                _cheeseFactory.Update(gameTime);
                UpdateLivesLeft();
                if (_leftButtonClicked)
                {
                    if (_cheeseFactory.PopCheeseAtPosition(new Vector2(_currentMouseState.X, _currentMouseState.Y)))
                    {
                        _popSoundEffect.Play();
                        _score++;
                    }
                }
                break;
            case GameState.GameOver:
                if (_leftButtonClicked)
                {
                    NewGame();
                    CurrentState = GameState.Playing;
                }
                break;
            default:
                break;
        }


        _previousMouseState = _currentMouseState;
        base.Update(gameTime);
    }

    private void UpdateLivesLeft()
    {
        _livesLeft = MAX_LIVES - _cheeseFactory.CheesesMissed;
        if (_livesLeft <= 0)
        {
            CurrentState = GameState.GameOver;
        }
    }

    private void ToggleFullScreenIfRightButtonClicked()
    {
        if (_rightButtonClicked)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }
    }

    private void ExitGameIfEscapeKeyPressed()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); }
    }

    private void UpdateButtonStates()
    {
        _currentMouseState = Mouse.GetState();

        _leftButtonClicked = (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released);

        _rightButtonClicked = (_currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released);
    }

    #endregion

    #region Update and related
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
                DrawGameOverScreen(gameTime);
                break;
            default:
                break;
        }

        // Draw game elements here

        _spriteBatch.End();
    }

    private void DrawGameOverScreen(GameTime gameTime)
    {
        var centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;

        var bobbingOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 20);
        _spriteBatch.Draw(_gameOverTexture, new Vector2(centerOfScreen.X - _gameOverTexture.Width / 2, centerOfScreen.Y - _gameOverTexture.Height / 2 + (float)bobbingOffset), Color.White);

        _spriteBatch.DrawString(_defaultFont, "Click to begin new game!", new Vector2(25, Window.ClientBounds.Height - 50), Color.Brown);
        DrawPoints(gameTime);
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
        DrawLivesLeft(gameTime);
        DrawPoints(gameTime);

        float mouseCursorScale = 1;
        if (_currentMouseState.LeftButton == ButtonState.Pressed)
        {
            mouseCursorScale = 0.8f;
        }

        //draw the crosshairs slightly smaller when the mouse button is down
        _spriteBatch.Draw(_crosshairsTexture, new Vector2(_currentMouseState.X - (_crosshairsTexture.Width * mouseCursorScale) / 2, _currentMouseState.Y - (_crosshairsTexture.Height * mouseCursorScale) / 2), null, Color.White, 0f, Vector2.Zero, mouseCursorScale, SpriteEffects.None, 0f);
    }

    private void DrawPoints(GameTime gameTime)
    {
        Vector2 pointTextSize = _defaultFont.MeasureString(_score.ToString());
        _spriteBatch.DrawString(_defaultFont, _score.ToString(), new Vector2(Window.ClientBounds.Width - _pointsTexture.Width - (pointTextSize.X + 40), 35), Color.Black, 0f, new Vector2(0, 0), 1.5f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(_pointsTexture, new Vector2(Window.ClientBounds.Width - _pointsTexture.Width - 25, 25), Color.White);
    }

    private void DrawLivesLeft(GameTime gameTime)
    {
        float heartScale = 0.5f;
        for (int i = 0; i < _livesLeft; i++)
        {
            _spriteBatch.Draw(_heartTexture,
                new Vector2(25 + i * (_heartTexture.Width * heartScale + 10), 25),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                heartScale,
                SpriteEffects.None,
                0f);
        }
    }
    #endregion
}