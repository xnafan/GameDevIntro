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
    private Texture2D _titleScreenTexture, _tileSpriteSheet, _wallTileSheet, _knightSpriteSheet, _logoSmall, _bonusOmeterTexture;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth = 25, _dungeonHeight = 15;
    private Vector2 _topLeftOfDungeon;
    private SpriteFont _defaultFont, _smallFont;
    private Song _backgroundMusic1, _backgroundMusic2;
    private List<SoundEffect> _swordSounds = new(), _deathSounds = new();
    private SoundEffect _chestOpen;
    private int _score;
    private float _timeLeft;
    private List<TextSprite> _floatingTexts = new();
    public GameState CurrentState { get; set; }
    private float _elapsedTimeSinceLastTimeDecrement = 0f;
    private BonusOMeter _bonusOMeter;
    #endregion

    #region Constructor and initialization
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
        _bonusOmeterTexture = Content.Load<Texture2D>("Graphics/bonusometer");

        _defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _smallFont = Content.Load<SpriteFont>("Fonts/SmallFont");

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
            (_graphics.PreferredBackBufferWidth - (_dungeonWidth * _tileSpriteSheet.Height)),
            (_graphics.PreferredBackBufferHeight - (_dungeonHeight * _tileSpriteSheet.Height))
        );

        _bonusOMeter = new BonusOMeter(new Vector2(60, 260), _bonusOmeterTexture, GraphicsDevice, 300);

        MediaPlayer.IsRepeating = true;
        PlayRandomMusic();
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
                if (_currentKeyboardState.GetPressedKeys().Length > 0){NewGame();}
                break;
            case GameState.Playing:
                DecrementTime(gameTime);
                MovePlayerBasedOnKeyboardState();
                _bonusOMeter.Update(_score, gameTime);
                break;
            case GameState.GameOver:
                break;
        }
        _floatingTexts.ForEach(ft => ft.Update(gameTime));
        _floatingTexts.RemoveAll(ft => ft.IsExpired);
        _previousKeyboardState = _currentKeyboardState;
    }

    private void DecrementTime(GameTime gameTime)
    {
        _elapsedTimeSinceLastTimeDecrement += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_elapsedTimeSinceLastTimeDecrement >= 1f)
        {
            _timeLeft = Math.Max(0, _timeLeft - 1);
            _elapsedTimeSinceLastTimeDecrement -= 1f;
        }
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
                _timeLeft += secondsToAdd;
                _floatingTexts.Add(new TextSprite($"+{secondsToAdd} sec", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + _topLeftOfDungeon + Vector2.UnitY * -32, new Vector2(0, -1), Color.Gold));
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
        var result = roll + _bonusOMeter.CurrentBonus;
        if (result > rollToBeat)
        {
            _floatingTexts.Add(new TextSprite($"Victory! ({result} > {rollToBeat})\n({roll}+{_bonusOMeter.CurrentBonus})", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + _topLeftOfDungeon + Vector2.UnitY * -32, new Vector2(0, -1), Color.LightGreen, 0.1f, 1500));
            _score += 15;
            _dungeon.ItemsLeft--;
            PlayRandomSwordSound();
            return true;
        }
        else
        {
            var damageTaken = Random.Shared.Next(6) + 1 + enemyDamageBonus;

            _deathSounds[Random.Shared.Next(_deathSounds.Count)].Play();
            _floatingTexts.Add(new TextSprite($"Defeat! ({result} =< {rollToBeat})", _defaultFont, new Vector2(_dungeon.PlayerPosition.X * _knightSpriteSheet.Height, _dungeon.PlayerPosition.Y * _knightSpriteSheet.Height) + _topLeftOfDungeon + Vector2.UnitY * -32, new Vector2(0, -1), Color.IndianRed, 0.1f, 1500));
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
        _score = 0;
        _timeLeft = 60;
        _hpLeft = MAX_HP;
        _bonusOMeter.Reset();
        CurrentState = GameState.Playing;
    }

    private void PlayRandomMusic()
    {
        if (Random.Shared.Next(2) == 0) { MediaPlayer.Play(_backgroundMusic1); }
        else { MediaPlayer.Play(_backgroundMusic2); }
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
                _dungeon.Draw(_spriteBatch, gameTime, _topLeftOfDungeon);
                DrawMiniLogo(gameTime);
                _bonusOMeter.Draw(_spriteBatch, gameTime);
                _floatingTexts.ForEach(ft => ft.Draw(_spriteBatch, gameTime));

                if (_bonusOMeter.CurrentBonus > -1)
                {
                    Vector2 destination = new Vector2(_topLeftOfDungeon.X + _tileSpriteSheet.Height * (_dungeon.PlayerPosition.X + 1), _topLeftOfDungeon.Y + _tileSpriteSheet.Height * (_dungeon.PlayerPosition.Y + 1)) - Vector2.One * 4;


                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0) continue;
                            Vector2 offsetDestination = destination + new Vector2(x * 2, y * 2);
                            _spriteBatch.DrawString(_smallFont, "+" + _bonusOMeter.CurrentBonus, offsetDestination, Color.Black * 0.5f);
                        }

                    }
                    _spriteBatch.DrawString(_smallFont, "+" + _bonusOMeter.CurrentBonus, destination, Color.White);

                }
                WriteItemsLeft();
                WriteScore();
                break;
        }


        _spriteBatch.End();
    }

    private void WriteItemsLeft()
    {
        // draw the score below the bonus o meter
        var itemsLeftText = $"{(int)_timeLeft} seconds left to clear {_dungeon.ItemsLeft} tiles. {_hpLeft} hp left";

        var textSize = _defaultFont.MeasureString(itemsLeftText);
        _spriteBatch.DrawString(_defaultFont, itemsLeftText, new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, 20), Color.White);
    }

    private void WriteScore()
    {
        // draw the score below the bonus o meter
        var scoreText = $"Score: {_score}";
        var scoreOffset = Vector2.UnitY * 20;

        var textSize = _defaultFont.MeasureString(scoreText);
        _spriteBatch.DrawString(_defaultFont, scoreText, new Vector2(_bonusOMeter.Position.X, _bonusOMeter.Position.Y + _bonusOMeter.Texture.Height) + scoreOffset, Color.White);
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
    #endregion
}