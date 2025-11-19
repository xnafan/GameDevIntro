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
        //get a reference to the graphics card,
        //for drawing and related functinality
        _graphics = new GraphicsDeviceManager(this);
        
        //set the base directory for loading content (graphics, sound effects, etc.)
        Content.RootDirectory = "Content";
        
        //hide the mouse
        IsMouseVisible = false;

        // Set resolution 
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //load the white square to use as the player
        _dieTexture = Content.Load<Texture2D>("gfx/tile");
        
        //find the center of the current screen
        var centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height)/2;
        
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

        //add updates of other objects here
        //...
        //...
    }

    private void SetDirectionFromKeyboard()
    {
        //set default direction to none
        var _desiredDirection = Vector2.Zero;
        
        //retrieve the state of all keys on the keyboard
        var state = Keyboard.GetState();

        //add all the directions, based on the four arrow keys or WASD
        if (state.IsKeyDown(Keys.Left) || (state.IsKeyDown(Keys.A))) { _desiredDirection -= Vector2.UnitX; }
        if (state.IsKeyDown(Keys.Right) || (state.IsKeyDown(Keys.D))) { _desiredDirection += Vector2.UnitX; }
        if (state.IsKeyDown(Keys.Up) || (state.IsKeyDown(Keys.W))) { _desiredDirection -= Vector2.UnitY; }
        if (state.IsKeyDown(Keys.Down) || (state.IsKeyDown(Keys.S))) { _desiredDirection += Vector2.UnitY; }

        //set the speed to one in the desired direction
        //to avoid having faster diagonal movement
        if (_desiredDirection != Vector2.Zero)
        {
            _desiredDirection.Normalize(); 
        }

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
        //...
        //...

        //end the draweing to the spritebatch
        _spriteBatch.End();
    }
}