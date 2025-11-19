using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDevIntro.GameBase;
public class StarterGame : Game
{
    #region Variables
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MovableObject _player;
    private Texture2D _dieTexture; 
    #endregion

    public StarterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //load the white square to use as the player
        _dieTexture = Content.Load<Texture2D>("gfx/tile");
        
        //find the center of the current screen
        var centerOfScreen = new Vector2(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height)/2;
        
        //instantiate a player object centered on the screen
        _player = new(centerOfScreen, _dieTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        SetDirectionFromKeyboard();

        //call the superclass' Update method
        base.Update(gameTime);

        //update the player
        _player.Update(gameTime);
    }

    private void SetDirectionFromKeyboard()
    {
        //set default direction to none
        var _desiredDirection = Vector2.Zero;
        
        //retrieve the state of all keys on the keyboard
        var state = Keyboard.GetState();

        //add all the directions, based on the four arrow keys or WASD
        if (state.IsKeyDown(Keys.Left) || (state.IsKeyDown(Keys.A))) { _desiredDirection += Vector2.UnitX * -1; }
        if (state.IsKeyDown(Keys.Right) || (state.IsKeyDown(Keys.D))) { _desiredDirection += Vector2.UnitX * 1; }
        if (state.IsKeyDown(Keys.Up) || (state.IsKeyDown(Keys.W))) { _desiredDirection += Vector2.UnitY * -1; }
        if (state.IsKeyDown(Keys.Down) || (state.IsKeyDown(Keys.S))) { _desiredDirection += Vector2.UnitY * 1; }

        //set the player's direction to the result of the input
        _player.Direction = _desiredDirection;
    }

    protected override void Draw(GameTime gameTime)
    {
        //clear the entire background 
        GraphicsDevice.Clear(Color.Navy);
        
        //call the superclass' Draw method
        base.Draw(gameTime);

        //start the drawingto the spritebatch (i.e. background)
        _spriteBatch.Begin();

        //draw the player
        _player.Draw(_spriteBatch, gameTime);
        
        //add other drawing here...

        //end the draweing to the spritebatch
        _spriteBatch.End();
    }
}