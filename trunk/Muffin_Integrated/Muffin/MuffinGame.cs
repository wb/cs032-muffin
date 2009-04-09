using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Definitions;
using Muffin.Components.Renderer;
using Muffin.Components.Physics;
namespace Muffin
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MuffinGame : Microsoft.Xna.Framework.Game
    {
        # region Game-Specific variables

        // The master objects lists
        private List<GameObject> _allObjects;
        private List<TerrainObject> _allTerrain;
        private List<AIObject> _allAIObjects;
        private List<PlayerObject> _allPlayers;

        // Anything that was upadated or moved in the last loop
        private List<GameObject> _updatedObjects;
        // Stuff that is being updated in the current cycle
        private List<GameObject> _updatingObjects;

        //GameComponents
        GameComponent _renderer;
        GameComponent _physics;

        // Class for loading levels
        XMLParser _xmlParser;

        // Flags for what's in _updatedObjects  (so you don't have to search it for specific types of objects)
        private bool _terrainChanged;
        private bool _AIChanged;
        private bool _playersChanged;
        private bool _otherChanged;

        private bool _terrainChanging;
        private bool _AIChanging;
        private bool _playersChanging;
        private bool _otherChanging;

        #endregion

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public MuffinGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _allObjects = new List<GameObject>();
            _allTerrain = new List<TerrainObject>();
            _allAIObjects = new List<AIObject>();
            _allPlayers = new List<PlayerObject>();


            LoadLevel();

            _renderer = new Renderer(this);
            Components.Add(_renderer);
            _renderer.UpdateOrder = 0;

            _physics = new Physics(this);
            Components.Add(_physics);
            _physics.UpdateOrder = 1;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _terrainChanged = _AIChanged = _playersChanged = _otherChanged = _terrainChanging = _AIChanging = _playersChanging = _otherChanging = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
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
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) || (this.quitGame) )
                this.Exit();

            beginTick();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        # region Game-specific methods
        
        protected void LoadLevel()
        {
            // load the xml file corresponding to the current level
            if (File.Exists("Content\\Levels\\" + GameConstants.CurrentLevel + ".xml"))
            {
                XmlDocument document = new XmlDocument();
                document.Load("Content\\Levels\\" + GameConstants.CurrentLevel + ".xml");
                _xmlParser = new XMLParser(document);
            }
            else
            {
                Console.WriteLine("The file " + "Content\\Levels\\" + GameConstants.CurrentLevel + ".xml" + " was not found");
            }

            _allObjects.Clear();
            _allTerrain.Clear();
            _allAIObjects.Clear();
            _allPlayers.Clear();

            // Load the current level
            List<GameObject> objs = new List<GameObject>();
            _xmlParser.loadLevel(objs, null);

            #region Box Testing and Forces

            /**
             * BEGIN BOX TESTING W/FORCES
             * */

            GameObject testBox1 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(100, 700, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 1000.0f, 1.0f);
            objs.Add(testBox1);
            //testBox1.applyForce(new Vector3(10000.0f, 0.0f, 0.0f), new Vector3(30, 30, 30));

            GameObject testBox2 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(200, 700, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 1000.0f, 1.0f);
            objs.Add(testBox2);
            //testBox2.applyForce(new Vector3(-10000.0f, 0.0f, 0.0f), new Vector3(30, 30, 30));

            //testBox1.applyForce(new Vector3(0.0f, 9.8f * testBox1.mass, 0.0f), new Vector3(60, 40, 30));

            /**
             * END BOX TESTING
             * */

            #endregion

            foreach (GameObject o in objs)
            {
                if (o is TerrainObject)
                    addTerrainObject(o as TerrainObject);
                else if (o is AIObject)
                    addAIObject(o as AIObject);
                else if (o is PlayerObject)
                    addPlayerObject(o as PlayerObject);
                else
                    _allObjects.Add(o);
            }
        }
        
        // For any global game book-keeping that needs to be done at the begining of every cycle
        protected void beginTick()
        {
            _updatedObjects = _updatingObjects;
            _terrainChanged = _playersChanging;
            _AIChanged = _AIChanging;
            _playersChanged = _playersChanging;
            _otherChanged = _otherChanging;

            _updatingObjects = new List<GameObject>();
            _AIChanging = false;
            _playersChanging = false;
            _playersChanging = false;
        }

        #endregion

        #region Object list modifiers

        // You must call one of these if it is necessary to add any objects to the game
        // DO NOT ADD DIRECTLY TO THE PUBLIC LISTS

        public void addTerrainObject(TerrainObject o)
        {
            _allObjects.Add(o);
            _allTerrain.Add(o as TerrainObject);
        }

        public void addAIObject(AIObject o)
        {
            _allObjects.Add(o);
            _allAIObjects.Add(o as AIObject);
        }

        public void addPlayerObject(PlayerObject o)
        {
            _allObjects.Add(o);
            _allPlayers.Add(o as PlayerObject);
        }

        public void addUpdateObject(GameObject o)
        {
            _updatingObjects.Add(o);

            if (o is TerrainObject)
                _terrainChanging = true;
            else if (o is AIObject)
                _AIChanging = true;
            else if (o is PlayerObject)
                _playersChanging = true;
            else
                _otherChanging = true;
        }

        #endregion

        #region Gets and sets

        public List<GameObject> allObjects { get { return _allObjects; } }
        public List<TerrainObject> allTerrain { get { return _allTerrain; } }
        public List<AIObject> allAI { get { return _allAIObjects; } }
        public List<PlayerObject> allPlayer { get { return _allPlayers; } }

        public List<GameObject> updated { get { return _updatedObjects; } }
        public bool terrainChanged { get { return _terrainChanged; } }
        public bool AIChanged { get { return _AIChanged; } }
        public bool playersChanged { get { return _playersChanged; } }
        public bool otherChanged { get { return _otherChanged; } }

        public bool quitGame { get; set; }

        #endregion

    }
}
