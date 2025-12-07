using GameDevIntro.SimpleZuul.Components;
using GameDevIntro.SimpleZuul.Model;
using GameDevIntro.SimpleZuul.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace GameDevIntro.SimpleZuul;
public class ZuulGame : Game
{
    #region Variables
    public enum GameState { TitleScreen, Playing, GameOver }
    private int _hpLeft = 25;
    const int MAX_HP = 25;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _titleScreenTexture, _tileSpriteSheet, _knightSpriteSheet, _logoSmall;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth = 29, _dungeonHeight = 17;
    private SpriteFont _defaultFont;
    private Song _backgroundMusic1, _backgroundMusic2;
    private SoundEffect _chestOpen, _swordSound, _deathSound;
    private List<TextSprite> _floatingTexts = new();
    public GameState CurrentState { get; set; }
    #endregion

    #region Constructor and initialization
    public ZuulGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        //_graphics.IsFullScreen = true;
        IsMouseVisible = false;
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _titleScreenTexture = Content.Load<Texture2D>("Graphics/TitleScreen");
        _tileSpriteSheet = Content.Load<Texture2D>("Graphics/sprite_sheet");
        _knightSpriteSheet = Content.Load<Texture2D>("Graphics/knight_spritesheet");
        _logoSmall = Content.Load<Texture2D>("Graphics/Zuul_logo_small");

        _defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");

        _backgroundMusic1 = Content.Load<Song>("Music/Action_3");

        _swordSound = Content.Load<SoundEffect>($"SoundEffects/sword_01");
        _deathSound = Content.Load<SoundEffect>($"SoundEffects/death_01");
        _chestOpen = Content.Load<SoundEffect>("SoundEffects/chest_opening");

        MediaPlayer.IsRepeating = true;
        PlayMusic();
    }
    #endregion

    #region Update() and related

    protected override void Update(GameTime gameTime)
    {
        _currentKeyboardState = Keyboard.GetState();

        if (_currentKeyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        if (_currentKeyboardState.IsKeyDown(Keys.F5) && _previousKeyboardState.IsKeyUp(Keys.F5))
        {
            NewGame();
        }
        if (_currentKeyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
            return;
        }


        base.Update(gameTime);
        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if (_currentKeyboardState.GetPressedKeys().Length > 0) { NewGame(); }
                break;
            case GameState.Playing:
                MovePlayerBasedOnKeyboardState();
                break;
            case GameState.GameOver:
                break;
        }
        _floatingTexts.ForEach(ft => ft.Update(gameTime));
        _floatingTexts.RemoveAll(ft => ft.IsExpired);
        _previousKeyboardState = _currentKeyboardState;
    }

    private void MovePlayerBasedOnKeyboardState()
    {
        Point oldPosition = _dungeon.PlayerPosition;
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
        if (direction != Point.Zero)
        {
            var newPosition = oldPosition + direction;
            tileType = _dungeon.Tiles[newPosition.X, newPosition.Y].Type;

            if (HandleTileInteraction(newPosition))
            {
                _dungeon.Tiles[newPosition.X, newPosition.Y].Type = Tile.TileType.Empty;
                _dungeon.PlayerPosition = newPosition;
            }
        }
        if (_dungeon.ItemsLeft <= 0)
        {
            CurrentState = GameState.GameOver;
        }
    }

    private bool HandleTileInteraction(Point newPosition)
    {
        Tile.TileType tileType = _dungeon.Tiles[newPosition.X, newPosition.Y].Type;

        switch (tileType)
        {
            case Tile.TileType.Empty:
                return true;
            case Tile.TileType.Chest:
                _dungeon.ItemsLeft--;
                var secondsToAdd = 5;
                _chestOpen.Play();
                _floatingTexts.Add(new TextSprite($"+{secondsToAdd} sec", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.Gold));
                return true;
            case Tile.TileType.Wall:
                return false;
            case Tile.TileType.Slime:
                return HandleCombat(1, 15, -1);
            case Tile.TileType.Skeleton:
                return HandleCombat(2, 25);
            case Tile.TileType.Dragon:
                return HandleCombat(4, 40, 3);
        }
        return true;
    }

    private bool HandleCombat(int rollToBeat, int pointsForVictory, int enemyDamageBonus = 0)
    {
        var roll = Random.Shared.Next(6) + 1;
        var result = roll;
        if (result > rollToBeat)
        {
            _floatingTexts.Add(new TextSprite($"Victory! ({result} > {rollToBeat})\n({roll}", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.LightGreen, 0.1f, 1500));
            _dungeon.ItemsLeft--;
            PlayRandomSwordSound();
            return true;
        }
        else
        {
            var damageTaken = Random.Shared.Next(6) + 1 + enemyDamageBonus;

            _deathSound.Play();
            _floatingTexts.Add(new TextSprite($"Defeat! ({result} =< {rollToBeat})", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.IndianRed, 0.1f, 1500));
            _hpLeft -= damageTaken;
        }
        if (_hpLeft <= 0)
        {
            CurrentState = GameState.GameOver;

        }
        return false;
    }

    private void PlayRandomSwordSound()
    {
        _swordSound.Play();
    }

    private bool KeyWasPressed(Keys right)
    {
        return _currentKeyboardState.IsKeyDown(right) && !_previousKeyboardState.IsKeyDown(right);
    }

    private void NewGame()
    {
        _dungeon = DungeonGenerator.GenerateDungeon(_dungeonWidth, _dungeonHeight, _tileSpriteSheet, _knightSpriteSheet);
        PlayMusic();
        _hpLeft = MAX_HP;
        CurrentState = GameState.Playing;
    }

    private void PlayMusic()
    {
        MediaPlayer.Play(_backgroundMusic1);
    }

    #endregion

    #region Draw() and related
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
        _spriteBatch.Begin();
        switch (CurrentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen();
                _spriteBatch.DrawString(_defaultFont, "WASD/Arrow keys - Move | F5 - New Game | F11 - Toggle Fullscreen | Esc - Quit", new Vector2(20, _graphics.PreferredBackBufferHeight - 40), Color.White);
                break;
            case GameState.Playing:
            case GameState.GameOver:
                _dungeon.Draw(_spriteBatch, gameTime, Vector2.Zero);
                DrawMiniLogo(gameTime);
                _floatingTexts.ForEach(ft => ft.Draw(_spriteBatch, gameTime));

                WriteItemsLeft();
                break;
        }


        _spriteBatch.End();
    }

    private void WriteItemsLeft()
    {
        // draw the score below the bonus o meter
        var itemsLeftText = $"{_dungeon.ItemsLeft} tiles. {_hpLeft} hp left";

        var textSize = _defaultFont.MeasureString(itemsLeftText);
        _spriteBatch.DrawString(_defaultFont, itemsLeftText, new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, 20), Color.White);
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

        _spriteBatch.Draw(_logoSmall, destinationRectangle, Color.White * 0.7f);
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
    #endregion
}