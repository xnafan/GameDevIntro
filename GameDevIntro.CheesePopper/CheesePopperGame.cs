using GameDevIntro.CheesePopper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
namespace GameDevIntro.MousePopper;
public class CheesePopperGame : Game
{
    #region Variables and enums
    public enum GameState { TitleScreen, Playing, GameOver }
    public GameState CurrentState { get; set; }
    private Vector2 _centerOfScreen;
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
    private float _timeSinceGameOver;
    #endregion

    #region Initialization
    public CheesePopperGame()
    {
        Content.RootDirectory = "Content";


        _graphics = new GraphicsDeviceManager(this);
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

        //load textures
        _logoTexture = Content.Load<Texture2D>("graphics/cheesePopper_logo_small");
        _cheeseTexture = Content.Load<Texture2D>("graphics/cheese");
        _crosshairsTexture = Content.Load<Texture2D>("graphics/crosshairs");
        _heartTexture = Content.Load<Texture2D>("graphics/heart");
        _gameOverTexture = Content.Load<Texture2D>("graphics/gameover");
        _pointsTexture = Content.Load<Texture2D>("graphics/points_128px");

        //load font
        _defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");

        //load sound effects
        _popSoundEffect = Content.Load<SoundEffect>("Sounds/pop");
        _failEffect = Content.Load<SoundEffect>("Sounds/fail");

        //load music
        _backgroundMusic = Content.Load<Song>("Sounds/music");

        //play music
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_backgroundMusic);

        NewGame();
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
        //pause the game if not active
        if (!IsActive)
        {
            return;
        }
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
                    var mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
                    if (_cheeseFactory.PopCheeseAtPosition(mousePosition))
                    {
                        _popSoundEffect.Play();
                        _score++;
                    }
                }
                break;

            case GameState.GameOver:
                _timeSinceGameOver += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeSinceGameOver < 1.0f)
                {
                    //wait a second before allowing restart
                    break;
                }
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
            _timeSinceGameOver = 0;
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

    #region Draw and related

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

        DrawCrosshairs();

        _spriteBatch.End();
    }

    private void DrawGameOverScreen(GameTime gameTime)
    {
        // Cache center of screen calculation
        if (_centerOfScreen == Vector2.Zero)
        {
            _centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;
        }

        var bobbingOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 20);
        _spriteBatch.Draw(_gameOverTexture, new Vector2(_centerOfScreen.X - _gameOverTexture.Width / 2, _centerOfScreen.Y - _gameOverTexture.Height / 2 + bobbingOffset), Color.White);

        _spriteBatch.DrawString(_defaultFont, "Click to begin new game!", new Vector2(25, Window.ClientBounds.Height - 50), Color.Brown);
        DrawPoints(gameTime);
    }

    private void DrawTitleScreen(GameTime gameTime)
    {
        // Cache center of screen calculation
        if (_centerOfScreen == Vector2.Zero)
        {
            _centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;
        }

        var bobbingOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3) * 20);
        _spriteBatch.Draw(_logoTexture, new Vector2(_centerOfScreen.X - _logoTexture.Width / 2, _centerOfScreen.Y - _logoTexture.Height / 2 + bobbingOffset), Color.White);

        _spriteBatch.DrawString(_defaultFont, "Click to begin game! Right click to toggle full screen", new Vector2(25, Window.ClientBounds.Height - 50), Color.Brown);
    }

    private void DrawGameScreen(GameTime gameTime)
    {
        _cheeseFactory.Draw(_spriteBatch, gameTime);
        DrawLivesLeft(gameTime);
        DrawPoints(gameTime);
    }

    private void DrawCrosshairs()
    {
        float mouseCursorScale = _currentMouseState.LeftButton == ButtonState.Pressed ? 0.8f : 1f;

        var crosshairPosition = new Vector2(
            _currentMouseState.X - (_crosshairsTexture.Width * mouseCursorScale) / 2,
            _currentMouseState.Y - (_crosshairsTexture.Height * mouseCursorScale) / 2);

        _spriteBatch.Draw(_crosshairsTexture, crosshairPosition, null, Color.White, 0f, Vector2.Zero, mouseCursorScale, SpriteEffects.None, 0f);
    }

    private void DrawPoints(GameTime gameTime)
    {

        // Recalculate position only when score changes
        Vector2 pointTextSize = _defaultFont.MeasureString(_score.ToString());

        //draw score and points texture in top right corner
        var _pointsPosition = new Vector2(Window.ClientBounds.Width - _pointsTexture.Width - 25, 25);
        var _scorePosition = new Vector2(_pointsPosition.X - pointTextSize.X - 25, _pointsPosition.Y + 10);

        _spriteBatch.DrawString(_defaultFont, _score.ToString(), _scorePosition, Color.Black, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(_pointsTexture, _pointsPosition, Color.White);
    }

    private void DrawLivesLeft(GameTime gameTime)
    {
        const float heartScale = 0.5f;
        const float heartSpacing = 10f;

        for (int i = 0; i < _livesLeft; i++)
        {
            var heartPosition = new Vector2(25 + i * (_heartTexture.Width * heartScale + heartSpacing), 25);
            _spriteBatch.Draw(_heartTexture, heartPosition, null, Color.White, 0f, Vector2.Zero, heartScale, SpriteEffects.None, 0f);
        }
    }
    #endregion
}