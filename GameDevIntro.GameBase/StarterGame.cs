using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameDevIntro.GameBase
{
    public class StarterGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MovableObject _player;
        private Texture2D _dieTexture;
        public StarterGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _dieTexture = Content.Load<Texture2D>("gfx/die");
            _player = new(Vector2.One * 200, Vector2.Zero, _dieTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            GetKeyboardInput();
            _player.Update(gameTime);

            base.Update(gameTime);
        }

        private void GetKeyboardInput()
        {
            var _desiredDirection = Vector2.Zero;
            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left)) { _desiredDirection += Vector2.UnitX * -1; }
            if (state.IsKeyDown(Keys.Right)) { _desiredDirection += Vector2.UnitX * 1; }
            if (state.IsKeyDown(Keys.Up)) { _desiredDirection += Vector2.UnitY * -1; }
            if (state.IsKeyDown(Keys.Down)) { _desiredDirection += Vector2.UnitY * 1; }

            _player.Direction = _desiredDirection;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
            _spriteBatch.Begin();
            _player.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();
        }
    }
}
