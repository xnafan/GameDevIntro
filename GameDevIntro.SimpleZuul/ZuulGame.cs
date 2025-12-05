using GameDevIntro.SimpleZuul.Model;
using GameDevIntro.SimpleZuul.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace GameDevIntro.SimpleZuul;
public class ZuulGame : Game
{
    public enum GameState { TitleScreen, Playing, GameOver }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _titleScreenTexture, _tileSpriteSheet, _wallTileSheet, _knightSpriteSheet, _logoSmall;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth = 25, _dungeonHeight = 15;
    private Vector2 _topLeftOfDungeon;
    private SpriteFont _defaultFont;
    private Song _backgroundMusic1, _backgroundMusic2;
    private List<SoundEffect> _swordSounds = new(), _deathSounds = new();
    private SoundEffect _chestOpen;
    private int _score;
    public GameState CurrentState { get; set; }
    private float _elapsedTimeSinceLastScoreDecrement = 0f;
    public ZuulGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.IsFullScreen = true;
        IsMouseVisible = false;
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _titleScreenTexture = Content.Load<Texture2D>("Graphics/TitleScreen");
        _tileSpriteSheet = Content.Load<Texture2D>("Graphics/sprite_sheet");
        _wallTileSheet = Content.Load<Texture2D>("Graphics/wall_tiles");
        _knightSpriteSheet = Content.Load<Texture2D>("Graphics/knight_spritesheet");
        _logoSmall = Content.Load<Texture2D>("Graphics/Zuul_logo_small");

        _defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");

        _backgroundMusic1 = Content.Load<Song>("Music/Action_3");
        _backgroundMusic2 = Content.Load<Song>("Music/Action_5");


        for (int i = 1; i <= 3; i++)
        {
            var swordSound = Content.Load<SoundEffect>($"SoundEffects/sword_{i:00}");
            _swordSounds.Add(swordSound);
        }
        for (int i = 1; i <= 2; i++)
        {
            var deathSound = Content.Load<SoundEffect>($"SoundEffects/death_{i:00}");
            _deathSounds.Add(deathSound);
        }

        _chestOpen = Content.Load<SoundEffect>("SoundEffects/chest_opening");

        _topLeftOfDungeon = new Vector2(
            (_graphics.PreferredBackBufferWidth - (_dungeonWidth * _tileSpriteSheet.Height)) ,
            (_graphics.PreferredBackBufferHeight - (_dungeonHeight * _tileSpriteSheet.Height)) 
        );

        
        MediaPlayer.IsRepeating = true;
        PlayRandomMusic();
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
                if (_currentKeyboardState.GetPressedKeys().Length > 0)
                {
                    
                    NewGame();
                }
                break;
            case GameState.Playing:
                DecrementScore(gameTime);
                MovePlayerBasedOnKeyboardState();
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        _previousKeyboardState = _currentKeyboardState;
    }

    private void DecrementScore(GameTime gameTime)
    {
        _elapsedTimeSinceLastScoreDecrement += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_elapsedTimeSinceLastScoreDecrement >= 1f)
        {
            _score = Math.Max(0, _score - 1);
            _elapsedTimeSinceLastScoreDecrement = 0f;
        }
    }

    private void MovePlayerBasedOnKeyboardState()
    {
        Tile.TileType tileType = Tile.TileType.Empty;
        Point direction = Point.Zero;

        if (KeyWasPressed(Keys.Up) || KeyWasPressed(Keys.W))
        {
            direction = new Point(0, -1);
        }
        else if (KeyWasPressed(Keys.Down) || KeyWasPressed(Keys.S))
        {
            direction = new Point(0, 1);
        }
        else if (KeyWasPressed(Keys.Left) || KeyWasPressed(Keys.A))
        {
            direction = new Point(-1, 0);
        }
        else if (KeyWasPressed(Keys.Right) || KeyWasPressed(Keys.D))
        {
            direction = new Point(1, 0);
        }
        tileType = _dungeon.MovePlayer(direction);

        switch (tileType)
        {
            case Tile.TileType.Slime:
            case Tile.TileType.Skeleton:
            case Tile.TileType.Dragon:
                PlayRandomSwordSound();
                break;
            default:
                break;
        }
        if (tileType == Tile.TileType.Chest)
        {
            _chestOpen.Play();
            _score += 10;
        }
    }

    private void PlayRandomSwordSound()
    {
        _swordSounds[Random.Shared.Next(_swordSounds.Count)].Play();
    }

    private bool KeyWasPressed(Keys right)
    {
        return _currentKeyboardState.IsKeyDown(right) && !_previousKeyboardState.IsKeyDown(right);
    }

    private void NewGame()
    {
        _dungeon = DungeonGenerator.GenerateDungeon(_dungeonWidth, _dungeonHeight, _tileSpriteSheet, _knightSpriteSheet);
        PlayRandomMusic();
        _score = 100;
        CurrentState = GameState.Playing;
    }

    private void PlayRandomMusic()
    {
        if (Random.Shared.Next(2) == 0) { MediaPlayer.Play(_backgroundMusic1); }
        else { MediaPlayer.Play(_backgroundMusic2); }
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
                DrawMiniLogo(gameTime);
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        _spriteBatch.DrawString(_defaultFont, $"Score: {_score}", new Vector2(20, 20), Color.White);

        _spriteBatch.End();
    }

    private void DrawMiniLogo(GameTime gameTime)
    {
        //calculate a destination rectangle for the logo with a scaling factor of 0.5

        var logoScale = 0.5f;
        var destinationRectangle = new Rectangle(
            25,
             25 + (int)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 350) * 10),
            (int)(_logoSmall.Width * logoScale),
            (int)(_logoSmall.Height * logoScale)
        );

        _spriteBatch.Draw(_logoSmall, destinationRectangle, Color.White);
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
