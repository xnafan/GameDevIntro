using GameDevIntro.GameBase.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace GameDevIntro.GameBase;
public class StarterGame : Game
{
    #region Variables
    private GraphicsDeviceManager _graphics;    //handle to graphics related functionality
    private SpriteBatch _spriteBatch;           //the "canvas" to draw to for visualizing game state
    private Sprite _player;                     //the player object, with location, movement, drawing, etc.
    private Texture2D _logoTexture;             //the image to use for the player
    private SpriteFont _defaultFont;            //the default font for drawing text
    #endregion
    
    #region Constructor and LoadingContent
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
        _logoTexture = Content.Load<Texture2D>("gfx/MonoGameLogo_64px");
        _defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");

        //start a new game
        NewGame();
    }

    private void NewGame()
    {
        //find the center of the current screen
        var centerOfScreen = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / 2;

        //instantiate a player object centered on the screen
        _player = new(centerOfScreen, _logoTexture);
    }
    #endregion

    #region Update and related methods
    protected override void Update(GameTime gameTime)
    {
        //call the superclass' Update method
        base.Update(gameTime);

        SetDirectionFromKeyboard();

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
        var keyboardState = Keyboard.GetState();

        //terminate application if ESC is pressed
        if (keyboardState.IsKeyDown(Keys.Escape)) { Exit(); }

        //add all the directions, based on the four arrow keys or WASD
        if (keyboardState.IsKeyDown(Keys.Up) || (keyboardState.IsKeyDown(Keys.W))) { _desiredDirection -= Vector2.UnitY; }
        if (keyboardState.IsKeyDown(Keys.Left) || (keyboardState.IsKeyDown(Keys.A))) { _desiredDirection -= Vector2.UnitX; }
        if (keyboardState.IsKeyDown(Keys.Down) || (keyboardState.IsKeyDown(Keys.S))) { _desiredDirection += Vector2.UnitY; }
        if (keyboardState.IsKeyDown(Keys.Right) || (keyboardState.IsKeyDown(Keys.D))) { _desiredDirection += Vector2.UnitX; }


        //set the speed to one in the desired direction
        //to avoid having faster diagonal movement
        if (_desiredDirection != Vector2.Zero)
        {
            _desiredDirection.Normalize();
        }

        //set the player's direction to the result of the input
        _player.Direction = _desiredDirection;
    }
    #endregion
    
    #region Draw and related methods
    protected override void Draw(GameTime gameTime)
    {
        //clear the entire background 
        GraphicsDevice.Clear(Color.White);

        //call the superclass' Draw method
        base.Draw(gameTime);

        //start drawing to the spritebatch (i.e. background)
        _spriteBatch.Begin();

        //draw the player
        _player.Draw(_spriteBatch, gameTime);

        //draw the player's coordinates in the top-left corner
        _spriteBatch.DrawString(_defaultFont, $"Player position: {_player.Position}", new Vector2(10, 10), Color.Black);

        //add other drawing here...
        //...
        //...

        //end the drawing to the spritebatch
        _spriteBatch.End();
    }
    #endregion
}