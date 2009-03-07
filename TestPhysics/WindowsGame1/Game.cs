using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using WindowsGame1;
using System.IO;



namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        float aspectRatio;
        List<Model> m_models;
        List<GameObject> m_game_object;
        MouseHandler m_mouse;
        XMLParser m_parser;

        // Set the position of the camera in world space, for our view matrix.
        GameCamera camera;

        public Game()
        {
            //renderer settings
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);

            this.IsMouseVisible = true;

            //3D model data structure initialization
            m_models = new List<Model>();
            m_game_object = new List<GameObject>();
            Content.RootDirectory = "Content";

            //mouse handler
            m_mouse = new MouseHandler(Mouse.GetState());

            //XML parser
            if (File.Exists("Content\\Levels\\level1.xml"))
            {
                XmlDocument document = new XmlDocument();
                document.Load("Content\\Levels\\level1.xml");
                m_parser = new XMLParser(document);
            }
            else
            {
                Console.WriteLine("The file " + "Content\\Levels\\level1.xml" + " was not found");
            }
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            int quality = 0;
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            // Check for 4xAA
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.FourSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.FourSamples;
            }
            // Check for 2xAA
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.TwoSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.TwoSamples;
            }
            return;

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
        /// initializeModelParameters will take go through each "static"
        /// model and enable features such as lighting and default projection
        /// matrices (subject to change)
        /// </summary>
        private void initializeModelParameters()
        {
            foreach (Model myModel in m_models)
            {
                // Copy any parent transforms.
                Matrix[] transforms = new Matrix[myModel.Bones.Count];
                myModel.CopyAbsoluteBoneTransformsTo(transforms);

                //initialize parameters for each model only once
                foreach (ModelMesh mesh in myModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.View = camera.ViewMatrix;
                        effect.Projection = camera.ProjectionMatrix;
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        GameObject test;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load Models into memory
            m_models.Add(Content.Load<Model>("Models\\flat"));
            m_models.Add(Content.Load<Model>("Models\\wedge"));
            m_models.Add(Content.Load<Model>("Models\\corner"));
            m_models.Add(Content.Load<Model>("Models\\inverted_corner"));

            m_parser.loadLevel(m_game_object, m_models);

            //m_game_object.Add(new GameObject((Model)m_models.ElementAt((int) ModelName.FLAT), ModelType.TERRAIN,
           // new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)));

            //m_game_object.Add(new GameObject((Model)m_models.ElementAt((int)ModelName.CORNER), ModelType.TERRAIN,
            //new Vector3(-30.0f, 0.0f, 30.0f), new Vector3(0.0f, 0.0f, 0.0f)));

            //m_game_object.Add(new GameObject((Model)m_models.ElementAt((int)ModelName.FLAT), ModelType.TERRAIN,
            //new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)));

            test = new GameObject((Model)m_models.ElementAt((int)ModelName.CORNER), ModelType.TERRAIN,
            new Vector3(60.0f, 100.0f, 60.0f), Quaternion.Identity);
            // TODO: use this.Content to load your game content here
            m_game_object.Add(test);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            //Camera initialization
            camera = new GameCamera(new Vector3(-400.0f, 400.0f, -400.0f), new Vector3(300.0f, 10.0f, 240.0f), aspectRatio);
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

            // physics processing
            foreach (GameObject o in m_game_object)
            {
                o.updatePosition((float) gameTime.ElapsedGameTime.TotalSeconds);
            }
            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            m_mouse.updateHandler(Mouse.GetState());

            float xRot = MathHelper.ToRadians((float)m_mouse.getNetY()) / 4.0f;
            float yRot = MathHelper.ToRadians((float)m_mouse.getNetX()) / 4.0f;

            Vector3 cameraRotation = new Vector3(xRot, yRot, 0.0f);

            int scrollValue = m_mouse.getNetScroll();
            camera.zoom(scrollValue);

            // TODO: Add your update logic here
            if (m_mouse.shouldRotate())
                cameraRotation = new Vector3(xRot, yRot, 0.0f);
            else
                cameraRotation = Vector3.Zero;

            camera.Update(cameraRotation);


            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // Set the position of the model in world space, and set the rotation.

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            this.initializeModelParameters();

            foreach (GameObject o in m_game_object)
            {
                o.renderObject();
            }
            base.Draw(gameTime);
        }
    }
}
