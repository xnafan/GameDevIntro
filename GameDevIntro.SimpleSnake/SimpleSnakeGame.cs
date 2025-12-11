using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
namespace GameDevIntro.SimpleSnake;
public class SimpleSnakeGame : Game
{
    #region Variables
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private int _boardSizeInTiles = 19;
    private Snake _snake;
    private Texture2D _whiteTexture;
    private Vector2 _boardOffset = new Vector2(50, 50), _tileSize = Vector2.One * 32;
    private readonly List<Point> _borderTiles = new();
    private Point _apple;
    private int _movesPerSecond = 5, _score, _highScore;
    private SpriteFont _font, _titleFont;

    public static readonly Dictionary<Keys, Point> DirectionMappings = new()
    {
        { Keys.Up, new Point(0, -1) },
        { Keys.Down, new Point(0, 1) },
        { Keys.Left, new Point(-1, 0) },
        { Keys.Right, new Point(1, 0) },
    };
    public static readonly Dictionary<Point, Point> OppositeDirections = new()
    {
        { new Point(0, -1), new Point(0, 1) },
        { new Point(0, 1), new Point(0, -1) },
        { new Point(-1, 0), new Point(1, 0) },
        { new Point(1, 0), new Point(-1, 0) },
    };
    #endregion

    #region Constructor and LoadContent
    public SimpleSnakeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
        _whiteTexture.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
        NewGame();
    }
    #endregion

    #region Methods relating to starting new game
    private void NewGame()
    {
        CalculateBorderTopLeftOffset();
        CalculateBorderTilePositions();
        _snake = new Snake(_whiteTexture, new Point(_boardSizeInTiles / 2, _boardSizeInTiles / 2), GetRandomDirection(), _movesPerSecond, _tileSize, 2);
        PlaceAppleRandomly();
        _score = 0;
    }

    private Point GetRandomDirection() => DirectionMappings.Values.ToList()[Random.Shared.Next(DirectionMappings.Count)];

    private void CalculateBorderTilePositions()
    {
        _borderTiles.Clear();
        for (int x = 0; x < _boardSizeInTiles; x++)
        {
            for (int y = 0; y < _boardSizeInTiles; y++)
            {
                if (x == 0 || x == _boardSizeInTiles - 1 || y == 0 || y == _boardSizeInTiles - 1)
                {
                    _borderTiles.Add(new Point(x, y));
                }
            }
        }
    }

    private void CalculateBorderTopLeftOffset()
    {
        var totalBoardSize = _tileSize * _boardSizeInTiles;
        _boardOffset = new Vector2(
            (_graphics.PreferredBackBufferWidth - totalBoardSize.X) / 2,
            (_graphics.PreferredBackBufferHeight - totalBoardSize.Y) / 2);
    }
    #endregion

    #region Methods relating to Update()
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); }

        // Change direction if key is pressed
        foreach (var mapping in DirectionMappings)
        {
            //if one of the direction keys is pressed
            if (Keyboard.GetState().IsKeyDown(mapping.Key))
            {
                // Prevent reversing direction
                if (mapping.Value != OppositeDirections[_snake.Direction])
                {
                    _snake.DesiredDirection = mapping.Value;
                }
            }
        }
        _snake.Update(gameTime);
        if (SnakeHeadCollidedWithBorder() || SnakeHeadCollidedWithBody()) { NewGame(); }
        CheckForSnakeEatingApple();
    }

    private bool SnakeHeadCollidedWithBody() => _snake.IsHeadCollidingWithBody();

    private void CheckForSnakeEatingApple()
    {
        if (_snake.BodyOverlapsPosition(_apple))
        {
            _snake.Grow();
            _score++;
            if (_score > _highScore) { _highScore = _score; }
            PlaceAppleRandomly();
        }
    }

    private void PlaceAppleRandomly() => _apple = GetRandomPointOnBoardNotOnSnake();

    private bool SnakeHeadCollidedWithBorder()
    {
        var headPosition = _snake.HeadPosition;
        return headPosition.X <= 0 || headPosition.X >= _boardSizeInTiles - 1 ||
               headPosition.Y <= 0 || headPosition.Y >= _boardSizeInTiles - 1;
    }

    private Point GetRandomPointOnBoardNotOnSnake()
    {
        Point newPoint;
        do
        {
            newPoint = new Point(Random.Shared.Next(1, _boardSizeInTiles - 1), Random.Shared.Next(1, _boardSizeInTiles - 1));
        } while (_snake.BodyOverlapsPosition(newPoint));
        return newPoint;
    }
    #endregion

    #region Methods relating to Draw()
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        GraphicsDevice.Clear(Color.Silver);

        _spriteBatch.Begin();
        DrawBorder();
        _snake.Draw(_spriteBatch, _boardOffset);
        DrawApple();
        WriteGameTitleAndScore();
        _spriteBatch.End();
    }

    private void DrawApple()
    {
        var appleDrawPosition = _boardOffset + new Vector2(_apple.X * _tileSize.X, _apple.Y * _tileSize.Y);
        _spriteBatch.Draw(_whiteTexture, new Rectangle((int)appleDrawPosition.X, (int)appleDrawPosition.Y, (int)_tileSize.X, (int)_tileSize.Y), Color.Red);
    }

    private void WriteGameTitleAndScore()
    {
        // Draw title centered between top border and top of window
        var titleText = "Simple Snake Game";
        var titleSize = _titleFont.MeasureString(titleText);
        _spriteBatch.DrawString(_titleFont, titleText, new Vector2((_graphics.PreferredBackBufferWidth - titleSize.X) / 2, (_boardOffset.Y - titleSize.Y) / 2), Color.Black);

        // Draw score centered at the bottom, centered horizontally and vertically centered in the space below the board
        var scoreText = $"Score: {_score} (highScore: {_highScore})";
        var scoreSize = _font.MeasureString(scoreText);
        _spriteBatch.DrawString(_font, scoreText, new Vector2((_graphics.PreferredBackBufferWidth - scoreSize.X) / 2, _boardOffset.Y + _tileSize.Y * _boardSizeInTiles + ((_graphics.PreferredBackBufferHeight - (_boardOffset.Y + _tileSize.Y * _boardSizeInTiles)) - scoreSize.Y) / 2), Color.Black);

    }

    private void DrawBorder()
    {
        foreach (var borderTile in _borderTiles)
        {
            var drawPosition = _boardOffset + new Vector2(borderTile.X * _tileSize.X, borderTile.Y * _tileSize.Y);
            _spriteBatch.Draw(_whiteTexture, new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)_tileSize.X, (int)_tileSize.Y), Color.Black);
        }
    }

    #endregion
}