using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RubeLoader;

namespace RUBELoaderTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RubeScene _rubeScene;
        private DebugViewXNA _physicsDebug;
        private Camera _camera;

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
            _rubeScene = new RubeScene(@"..\..\..\..\..\RubeData\basictest.json", Content, GraphicsDevice);
            _camera = new Camera(GraphicsDevice.Viewport);

            _physicsDebug = new DebugViewXNA(_rubeScene.World);
            _physicsDebug.LoadContent(this.GraphicsDevice, this.Content);
            _physicsDebug.AppendFlags(DebugViewFlags.Shape);
            _physicsDebug.AppendFlags(DebugViewFlags.Controllers);
            _physicsDebug.AppendFlags(DebugViewFlags.DebugPanel);
            _physicsDebug.AppendFlags(DebugViewFlags.PerformanceGraph);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _rubeScene.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _rubeScene.Draw();

            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width/50, GraphicsDevice.Viewport.Height/50, 0f, 0f, 1f);
            Matrix view = _camera.GetViewMatrix(Vector2.One);
            _physicsDebug.RenderDebugData(ref proj, ref view);

            base.Draw(gameTime);
        }
    }
}
