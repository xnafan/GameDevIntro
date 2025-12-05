using GameDevIntro.SimpleZuul.Model;
using GameDevIntro.SimpleZuul.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDevIntro.SimpleZuul;
public class ZuulGame : Game
{
    public enum GameState {TitleScreen, Playing, GameOver }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _titleScreenTexture;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth, _dungeonHeight;

    public GameState CurrentState { get; set; }

    public ZuulGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _titleScreenTexture = Content.Load<Texture2D>("Graphics/TitleScreen");
    }

    protected override void Update(GameTime gameTime)
    {
        _currentKeyboardState = Keyboard.GetState();
        
        if (_currentKeyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if(_currentKeyboardState.GetPressedKeys().Length > 0)
                {
                    CurrentState = GameState.Playing;
                    NewGame();
                }
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        _previousKeyboardState = _currentKeyboardState;
    }

    private void NewGame()
    {
        _dungeon = DungeonGenerator.GenerateDungeon(_dungeonWidth, _dungeonHeight);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
        _spriteBatch.Begin();
        switch (CurrentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen();
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        _spriteBatch.End();
    }

    private void DrawTitleScreen()
    {
        _spriteBatch.Draw(_titleScreenTexture, destinationRectangle: new Rectangle(
0,
0,
_graphics.PreferredBackBufferWidth,
_graphics.PreferredBackBufferHeight
), Color.White);
    }
}
