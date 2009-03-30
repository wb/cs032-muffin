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
        Physics.PhysicsEngine _physicsEngine;

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
            if (File.Exists("Content\\Levels\\level.xml"))
            {
                XmlDocument document = new XmlDocument();
                document.Load("Content\\Levels\\level.xml");
                m_parser = new XMLParser(document);
            }
            else
            {
                Console.WriteLine("The file " + "Content\\Levels\\level.xml" + " was not found");
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

            _physicsEngine = new Physics.Physics();

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
        GameObject test, test2;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load Models into memory
            m_models.Add(Content.Load<Model>("Models\\flat"));
            m_models.Add(Content.Load<Model>("Models\\wedge"));
            m_models.Add(Content.Load<Model>("Models\\corner"));
            m_models.Add(Content.Load<Model>("Models\\inverted_corner"));
            m_models.Add(Content.Load<Model>("Models\\centered_cube"));

            m_parser.loadLevel(m_game_object, m_models);


            /**
             * CREATE THE TEST OBJECT
             * 
             * */
            /*
            // the vector is the position
            test = new GameObject((Model)m_models.ElementAt((int)ModelName.BOX), ModelType.OBJECT, new Vector3(210.0f, 300.0f, 210.0f), Quaternion.Identity, new Vector3(60,60,60));
            m_game_object.Add(test);

            // the vector is the position
            test2 = new GameObject((Model)m_models.ElementAt((int)ModelName.BOX), ModelType.OBJECT, new Vector3(310.0f, 300.0f, 210.0f), Quaternion.Identity, new Vector3(60, 60, 60));
            m_game_object.Add(test2);

            test2.applyForce(new Vector3(-10000, 0, 0), Vector3.Zero);
            
            // apply the force: force as a vector first, then location where force is applied
            //(relative to center at 0,0,0, with vertices between (-30,-30,-30) and (30,30,30)
            //test.applyForce(new Vector3(0, 10, 0), new Vector3(0, 30, 30));
          */
            Random randomClass = new Random();

            for (int whatever = 0; whatever < 10; whatever++)
            {
                GameObject thing = new GameObject((Model)m_models.ElementAt((int)ModelName.BOX), ModelType.OBJECT, new Vector3((float) randomClass.Next(0, 600), (float) randomClass.Next(600, 1200), (float) randomClass.Next(0, 600)), Quaternion.Identity, new Vector3(60, 60, 60));
                m_game_object.Add(thing);

                thing.applyForce(new Vector3(randomClass.Next(-10000, 10000), randomClass.Next(-10000, 10000), randomClass.Next(-10000, 10000)), Vector3.Zero);
                
            }
            /*
             * END TEST OBJECT
             * 
             * */




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

            _physicsEngine.update(m_game_object, (float)gameTime.ElapsedGameTime.TotalSeconds);
            
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
