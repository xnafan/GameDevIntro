using GameDevIntro.DodgeWall.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameDevIntro.DodgeWall;
public class DodgeWallGame : Game
{
    #region Variables and Properties
    private GraphicsDeviceManager graphics;
    private Texture2D _box64, _box32;
    public static List<BlockLine> BlockLines = new List<BlockLine>();
    public static Rectangle ScreenBoundary { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static Random Random = new Random();
    private Player _player;
    #endregion

    public DodgeWallGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        graphics.PreferredBackBufferHeight = 768;
        graphics.PreferredBackBufferWidth = 1024;
    }

    void StartNewGame()
    {
        CreateBlockLines();
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        Vector2 centeredCloseToBottom = new Vector2(ScreenBoundary.Width / 2, ScreenBoundary.Height * .6f);
        _player = new Player(centeredCloseToBottom, _box32);
    }

    private void CreateBlockLines()
    {
        for (int i = 0; i < 2; i++)
        {
            var newBlockLine =
                new BlockLine(-Vector2.UnitY * ScreenBoundary.Height / 2f * i, _box64);
            BlockLines.Add(newBlockLine);
        }
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        _box64 = Content.Load<Texture2D>("graphics/box_64");
        _box32 = Content.Load<Texture2D>("graphics/box_32");
        ScreenBoundary = GraphicsDevice.PresentationParameters.Bounds;
        StartNewGame();
    }

    protected override void Update(GameTime gameTime)
    {
        IfEscapePressedThenExitGame();
        MoveBlockLinesAndPushPlayerIfNeeded(gameTime);
        _player.Update(gameTime);
    }

    private void IfEscapePressedThenExitGame()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); }
    }

    private void MoveBlockLinesAndPushPlayerIfNeeded(GameTime gameTime)
    {
        foreach (var line in BlockLines)
        {
            line.Update(gameTime);
            PushPlayerIfNeeded(line);
            IfLineIsBelowScreenMoveItAboveScreen(line);
        }
    }

    private void PushPlayerIfNeeded(BlockLine line)
    {
        Rectangle playerBounds = _player.GetBounds();
        while (line.Overlaps(playerBounds))
        {
            _player.Position += Vector2.UnitY / 2;
            playerBounds = _player.GetBounds();
        }
    }

    private static void IfLineIsBelowScreenMoveItAboveScreen(BlockLine line)
    {
        if (line.Position.Y > ScreenBoundary.Height)
        {
            line.MoveAboveScreenAndOpenARandomHole();
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Navy);
        SpriteBatch.Begin();
        BlockLines.ForEach(line => line.Draw());
        _player.Draw();
        SpriteBatch.End();
    }
}
