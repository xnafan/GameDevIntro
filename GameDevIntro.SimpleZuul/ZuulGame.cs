using GameDevIntro.SimpleZuul.Model;
using GameDevIntro.SimpleZuul.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameDevIntro.SimpleZuul;
public class ZuulGame : Game
{
    public enum GameState {TitleScreen, Playing, GameOver }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _titleScreenTexture, _tileSpriteSheet, _wallTileSheet, _knightSpriteSheet;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth = 25, _dungeonHeight = 15;
    private Vector2 _topLeftOfDungeon;
    public GameState CurrentState { get; set; }

    public ZuulGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.IsFullScreen = true;
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _titleScreenTexture = Content.Load<Texture2D>("Graphics/TitleScreen");
        _tileSpriteSheet = Content.Load<Texture2D>("Graphics/sprite_sheet");
        _wallTileSheet = Content.Load<Texture2D>("Graphics/wall_tiles");
        _knightSpriteSheet = Content.Load<Texture2D>("Graphics/knight_spritesheet");

        _topLeftOfDungeon = new Vector2(
            (_graphics.PreferredBackBufferWidth - (_dungeonWidth * _tileSpriteSheet.Height)) / 2,
            (_graphics.PreferredBackBufferHeight - (_dungeonHeight * _tileSpriteSheet.Height)) / 2
        );
    }

    protected override void Update(GameTime gameTime)
    {
        _currentKeyboardState = Keyboard.GetState();
        
        if (_currentKeyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        if (_currentKeyboardState.IsKeyDown(Keys.F5))
        {
            NewGame();
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
                MovePlayerBasedOnKeyboardState();
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        _previousKeyboardState = _currentKeyboardState;
    }

    private void MovePlayerBasedOnKeyboardState()
    {
        if(KeyWasPressed(Keys.Up))
        {
            _dungeon.MovePlayer(0, -1);
        }
        else if (KeyWasPressed(Keys.Down))
        {
            _dungeon.MovePlayer(0, 1);
        }
        else if (KeyWasPressed(Keys.Left))
        {
            _dungeon.MovePlayer(-1, 0);
        }
        else if (KeyWasPressed(Keys.Right))
        {
            _dungeon.MovePlayer(1, 0);
        }    
    }

   

    private bool KeyWasPressed(Keys right)
    {
        return _currentKeyboardState.IsKeyDown(right) && !_previousKeyboardState.IsKeyDown(right);
    }

    private void NewGame()
    {
        _dungeon = DungeonGenerator.GenerateDungeon(_dungeonWidth, _dungeonHeight, _tileSpriteSheet, _knightSpriteSheet);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
        _spriteBatch.Begin();
        switch (CurrentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen();
                break;
            case GameState.Playing:
                _dungeon.Draw(_spriteBatch, gameTime, _topLeftOfDungeon);
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
