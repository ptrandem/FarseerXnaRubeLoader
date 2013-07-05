using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RUBELoaderTest.Entities;
using RubeLoader;

namespace RUBELoaderTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RubeScene _rubeScene;
        private DebugViewXNA _physicsDebug;
        private Camera _camera;
        private Character _character;
        private SpriteFont _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
                {
                    PreferredBackBufferHeight = 900,
                    PreferredBackBufferWidth = 1400
                };
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _rubeScene = new RubeScene(@"..\..\..\..\..\RubeData\cartest.json", Content, GraphicsDevice);
            _camera = new Camera(GraphicsDevice.Viewport);
            _font = Content.Load<SpriteFont>("font");

            _physicsDebug = new DebugViewXNA(_rubeScene.World);
            _physicsDebug.LoadContent(this.GraphicsDevice, this.Content);
            _physicsDebug.AppendFlags(DebugViewFlags.Shape);
            _physicsDebug.AppendFlags(DebugViewFlags.Controllers);
            _physicsDebug.AppendFlags(DebugViewFlags.DebugPanel);
            _physicsDebug.AppendFlags(DebugViewFlags.PerformanceGraph);

            _character = new Character();
            _rubeScene.AttachJointControllers(_character, "characterWheel");
            _rubeScene.AttachBodyControllers(_character, "chacterbody");
            _character.Init();


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyState.IsKeyDown(Keys.Up))
            {
                _camera.Position -= new Vector2(0, 0.1f);
            }

            if (keyState.IsKeyDown(Keys.Down))
            {
                _camera.Position += new Vector2(0, 0.1f);
            }

            if (keyState.IsKeyDown(Keys.Left))
            {
                _camera.Position -= new Vector2(0.1f, 0);
            }

            if (keyState.IsKeyDown(Keys.Right))
            {
                _camera.Position += new Vector2(0.1f, 0);
            }

            if(keyState.IsKeyDown(Keys.A))
            {
                _character.MoveLeft();
            }

            if (keyState.IsKeyDown(Keys.D))
            {
                _character.MoveRight();
            }

            if (keyState.IsKeyDown(Keys.W))
            {
                _character.Jump();
            }

            if (keyState.IsKeyDown(Keys.OemMinus))
            {
                _camera.Zoom -= 0.001f;
                //_camera.Position = new Vector2(_camera.Position.X + (_camera.Zoom * .9f)
                //    , _camera.Position.Y + (_camera.Zoom * .5f));
                
            }

            if (keyState.IsKeyDown(Keys.OemPlus))
            {
                _camera.Zoom += 0.001f;
            }

            _rubeScene.Update(gameTime);

            _character.Decay();

            _camera.Position = _character.Body.Position - new Vector2(4, 4);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0f, 0f, 0.1f,0xff));

            _spriteBatch.Begin();
            var location = new Vector2(_graphics.PreferredBackBufferWidth/2, _graphics.PreferredBackBufferHeight/2);
            _spriteBatch.DrawString(_font, string.Format("Camera Location (x: {0} y:{1}", _camera.Position.X, _camera.Position.Y), location, Color.White);
            _spriteBatch.DrawString(_font, string.Format("Car Location (x: {0} y:{1}",
                                                         (_character.Body.Position.X),
                                                         (_character.Body.Position.Y)),
                                    location + new Vector2(0, 10), Color.White);
            _spriteBatch.End();

            _rubeScene.Draw();
            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width/100, GraphicsDevice.Viewport.Height/100, 0f, 0f, 1f);
            Matrix view = _camera.GetViewMatrix(Vector2.One);
            _physicsDebug.RenderDebugData(ref proj, ref view);

            base.Draw(gameTime);
        }
    }
}
