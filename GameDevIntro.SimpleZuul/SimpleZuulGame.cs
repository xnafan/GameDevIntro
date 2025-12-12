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
namespace GameDevIntro.SimpleZuul;
public class SimpleZuulGame : Game
{
    #region Variables
    public enum GameState { TitleScreen, Playing, GameLost, GameWon }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _titleScreenTexture, _tileSpriteSheet, _knightSpriteSheet, _logoSmall, _difficultyLegend, _whiteTexture, _victoryTexture;
    private KeyboardState _currentKeyboardState, _previousKeyboardState;
    private Dungeon _dungeon;
    private readonly int _dungeonWidth = 27, _dungeonHeight = 17;
    private SpriteFont _defaultFont, _smallFont;
    private Song _backgroundMusic1;
    private SoundEffect _chestOpen, _swordSound, _deathSound;
    private List<TextSprite> _floatingTexts = new();
    private Player _player = new();
    private int _numberOfRedScreens;

    public GameState CurrentState { get; set; }
    #endregion

    #region Constructor and initialization
    public SimpleZuulGame()
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
        _difficultyLegend = Content.Load<Texture2D>("Graphics/difficulty_192px");
        _victoryTexture = Content.Load<Texture2D>("Graphics/Victory");

        _defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _smallFont = Content.Load<SpriteFont>("Fonts/SmallFont");

        _backgroundMusic1 = Content.Load<Song>("Music/Action_3");

        _swordSound = Content.Load<SoundEffect>($"SoundEffects/sword_01");
        _deathSound = Content.Load<SoundEffect>($"SoundEffects/death_01");
        _chestOpen = Content.Load<SoundEffect>("SoundEffects/chest_opening");

        _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
        _whiteTexture.SetData(new[] { Color.White });

        MediaPlayer.IsRepeating = true;
        PlayMusic();
    }
    #endregion

    #region Update() and related

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _currentKeyboardState = Keyboard.GetState();

        if (_currentKeyboardState.IsKeyDown(Keys.Escape)) { Exit(); }
        if (KeyWasPressed(Keys.Enter)) { NewGame(); }
        if (_currentKeyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
            return;
        }

        switch (CurrentState)
        {
            case GameState.TitleScreen:
                if (_currentKeyboardState.GetPressedKeys().Length > 0) { NewGame(); }
                break;
            case GameState.Playing:
                MovePlayerBasedOnKeyboardState();
                break;
        }

        UpdateFloatingTexts(gameTime);
        _previousKeyboardState = _currentKeyboardState;
    }

    private void UpdateFloatingTexts(GameTime gameTime)
    {

        _floatingTexts.ForEach(ft => ft.Update(gameTime));
        _floatingTexts.RemoveAll(ft => ft.IsExpired);
    }

    private void MovePlayerBasedOnKeyboardState()
    {
        Point oldPosition = _dungeon.Player.Position;
        Tile.TileType tileType = Tile.TileType.Empty;
        Point direction = Point.Zero;

        if (KeyWasPressed(Keys.Up) || KeyWasPressed(Keys.W)) { direction = new Point(0, -1); }
        else if (KeyWasPressed(Keys.Down) || KeyWasPressed(Keys.S)) { direction = new Point(0, 1); }
        else if (KeyWasPressed(Keys.Left) || KeyWasPressed(Keys.A)) { direction = new Point(-1, 0); }
        else if (KeyWasPressed(Keys.Right) || KeyWasPressed(Keys.D)) { direction = new Point(1, 0); }
        if(KeyWasPressed(Keys.Space)){ 
            CurrentState = GameState.GameWon; 
//            _player.AttackRollBonus++;
            }
        if (direction != Point.Zero)
        {
            var newPosition = oldPosition + direction;
            tileType = _dungeon.Tiles[newPosition.X, newPosition.Y].Type;

            if (HandleTileInteraction(newPosition))
            {
                _dungeon.Tiles[newPosition.X, newPosition.Y].Type = Tile.TileType.Empty;
                _dungeon.Player.Position = newPosition;
            }
            if (_dungeon.ItemsLeft <= 0)
            {
                CurrentState = GameState.GameWon;
            }
        }
    }

    private bool HandleTileInteraction(Point newPosition)
    {
        // get the tile type at the new position
        Tile.TileType tileType = _dungeon.Tiles[newPosition.X, newPosition.Y].Type;

        // handle interaction based on tile type
        switch (tileType)
        {
            case Tile.TileType.Empty:
                return true;
            case Tile.TileType.Chest:
                _dungeon.ItemsLeft--;
                var hpToAdd = Random.Shared.Next(6) + 1;
                _chestOpen.Play();

                var chestContentRoll = Random.Shared.Next(6) + 1;
                if (_player.AttackRollBonus >= 2)
                {
                    chestContentRoll = 1;   //ensure no more strength bonuses
                }
                if (chestContentRoll < 5)
                {

                    _floatingTexts.Add(new TextSprite($"Health potion! +{hpToAdd} hp", _defaultFont, new Vector2(_dungeon.Player.Position.X * _knightSpriteSheet.Height, _dungeon.Player.Position.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.LightGreen, 0.1f, 2000));
                }
                else
                {
                    _player.AttackRollBonus++;
                    _floatingTexts.Add(new TextSprite($"Strength bonus! +1 to attack rolls", _defaultFont, new Vector2(_dungeon.Player.Position.X * _knightSpriteSheet.Height, _dungeon.Player.Position.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.Gold, 0.1f, 2000));
                }
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
        var result = roll + _player.AttackRollBonus;
        var resultText = $"{roll}";
        if (_player.AttackRollBonus > 0)
        {
            resultText += $" + {_player.AttackRollBonus}";
        }
        if (result > rollToBeat)
        {
            _floatingTexts.Add(new TextSprite($"{resultText} > {rollToBeat} = Victory! ", _defaultFont, new Vector2(_dungeon.Player.Position.X * _knightSpriteSheet.Height, _dungeon.Player.Position.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.LightGreen, 0.1f, 1500));
            _dungeon.ItemsLeft--;
            PlayRandomSwordSound();
            _player.Score += pointsForVictory;
            return true;
        }
        else
        {
            // enemy hits player
            // calculate damage, minimum of 1
            var damageTaken = Random.Shared.Next(6) + 1 + enemyDamageBonus;
            damageTaken = Math.Max(damageTaken, 1);

            _deathSound.Play();
            _floatingTexts.Add(new TextSprite($"{result} <= {rollToBeat} = Defeat!", _defaultFont, new Vector2(_dungeon.Player.Position.X * _knightSpriteSheet.Height, _dungeon.Player.Position.Y * _knightSpriteSheet.Height) + Vector2.UnitY * -32, new Vector2(0, -1), Color.IndianRed, 0.1f, 1500));
            _player.HitPoints -= damageTaken;
            _numberOfRedScreens = 3;
        }
        if (_player.HitPoints <= 0)
        {
            CurrentState = GameState.GameLost;
            _dungeon.Tiles[_dungeon.Player.Position.X, _dungeon.Player.Position.Y].Type = Tile.TileType.Tombstone;

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
        _dungeon = DungeonGenerator.GenerateDungeon(_dungeonWidth, _dungeonHeight, _player, _tileSpriteSheet, _knightSpriteSheet, _smallFont);
        PlayMusic();
        _player.Reset();
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
                _spriteBatch.DrawString(_defaultFont, "WASD/Arrow keys - Move | ENTER - New Game | F11 - Toggle Fullscreen | Esc - Quit", new Vector2(20, _graphics.PreferredBackBufferHeight - 40), Color.White);
                break;
            case GameState.Playing:
            case GameState.GameLost:
            case GameState.GameWon:
                _dungeon.Draw(_spriteBatch, gameTime);
                _floatingTexts.ForEach(ft => ft.Draw(_spriteBatch, gameTime));
                DrawDifficultyLegend();
                WriteScoreAndHP();
                DrawMiniLogo(gameTime);
                break;
        }
        if (_numberOfRedScreens > 0)
        {
            _spriteBatch.Draw(_whiteTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.Red * 0.2f * _numberOfRedScreens);
            _numberOfRedScreens = 0;
        }
        if (CurrentState == GameState.GameWon){DrawWonMessage(gameTime);}
        else if (CurrentState == GameState.GameLost) {
            //write "ENTER TO RESTART" centered on the screen
            var lostText = "YOU HAVE PERISHED! PRESS ENTER TO RESTART";
            var textSize = _defaultFont.MeasureString(lostText);
            var yOffset = (int)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500) * 5);
            //draw shadow   
            _spriteBatch.DrawString(_defaultFont, lostText, new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2 + 2, (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 + 2 + yOffset), Color.Black);
            _spriteBatch.DrawString(_defaultFont, lostText, new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 + yOffset) , Color.White);


        }
        _spriteBatch.End();
    }

    private void DrawWonMessage(GameTime gameTime)
    {
        //draw victory texture centered,  scaled to 50%    
        var scale = 0.5f;
        var destinationRectangle = new Rectangle(
            (_graphics.PreferredBackBufferWidth - (int)(_victoryTexture.Width * scale)) / 2,
            (_graphics.PreferredBackBufferHeight - (int)(_victoryTexture.Height * scale)) / 2 + (int)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 350) * 10),
            (int)(_victoryTexture.Width * scale),
            (int)(_victoryTexture.Height * scale)
        );
        _spriteBatch.Draw(_victoryTexture, destinationRectangle, Color.White);
    }

    private void DrawDifficultyLegend()
    {

        _spriteBatch.Draw(_difficultyLegend, new Vector2(_graphics.PreferredBackBufferWidth - _difficultyLegend.Width + 10, 250), Color.White);
    }

    private void WriteScoreAndHP()
    {
        // draw the score below the bonus o meter
        var itemsLeftText = $"{_player.Score} points | {_player.HitPoints} hp left";

        var textSize = _defaultFont.MeasureString(itemsLeftText);
        _spriteBatch.DrawString(_defaultFont, itemsLeftText, new Vector2((_graphics.PreferredBackBufferWidth - textSize.X) / 2, 20), Color.White);
    }

    private void DrawMiniLogo(GameTime gameTime)
    {
        //calculate a destination rectangle for the logo with a scaling factor of 0.5

        int xOffsetForDungeonWidth = _dungeonWidth * _tileSpriteSheet.Height - 7;
        var logoScale = 0.4f;
        var destinationRectangle = new Rectangle(
            xOffsetForDungeonWidth,
             15 + (int)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 350) * 10),
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