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
using Muffin.Components.UI;
using Muffin.Components.AI;
using Muffin.Objects;

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

        // Objects that are being removed from the world
        private List<GameObject> _removedObjects;
        private List<GameObject> _removingObjects;

        // Level stuff
        private List<LevelObject> _levels;
        private int _currentLevel = 0;

        //GameComponents
        GameComponent _renderer, _physics, _inputManager, _ai, _menuComponent;

        // Class for loading levels
        XMLParser _xmlParser;

        // for pausing/game over
        private Boolean _paused;
        private Boolean _gameOver;

        // Flags for what's in _updatedObjects  (so you don't have to search it for specific types of objects)
        private bool _terrainChanged;
        private bool _AIChanged;
        private bool _playersChanged;
        private bool _objectsRemoved;

        private bool _terrainChanging;
        private bool _AIChanging;
        private bool _playersChanging;
        private bool _objectsRemoving;

        private GameCamera _camera;

        private Effect textureDraw;

        #endregion

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        private SoundManager _soundManager;

        public MuffinGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _allObjects = new List<GameObject>();
            _allTerrain = new List<TerrainObject>();
            _allAIObjects = new List<AIObject>();
            _allPlayers = new List<PlayerObject>();

            _updatedObjects = new List<GameObject>();
            _updatingObjects = new List<GameObject>();

            _removedObjects = new List<GameObject>();
            _removingObjects = new List<GameObject>();

            // create some levels
            _levels = new List<LevelObject>();
            _levels.Add(new LevelObject("level_flat"));
            _levels.Add(new LevelObject("level_tall"));
            _levels.Add(new LevelObject("level_terrain"));
            _levels.Add(new LevelObject("level1"));

            LoadLevel(0);

            _renderer = new Renderer(this);
            Components.Add(_renderer);
            _renderer.UpdateOrder = 0;

            _inputManager = new InputManager(this);
            Components.Add(_inputManager);
            _inputManager.UpdateOrder = 1;

            _menuComponent = new MenuComponent(this);
            Components.Add(_menuComponent);
            _menuComponent.UpdateOrder = 2;

            _physics = new Physics(this);
            Components.Add(_physics);
            _physics.UpdateOrder = 3;

            _ai = new AI(this);
            Components.Add(_ai);
            _ai.UpdateOrder = 4;

            _paused = false;
            _gameOver = false;


            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _terrainChanged = _AIChanged = _playersChanged = _terrainChanging = _AIChanging = _playersChanging = 
                _objectsRemoved = _objectsRemoving = false;

            // create a new camera
            _camera = new GameCamera(_allPlayers.ElementAt(0), 24 * new Vector3(-20, 40, -20), new Vector3(0, 0, 0), graphics.GraphicsDevice.Viewport.AspectRatio);
            // and pass it to the renderer
            ((Renderer)_renderer).SetUpCamera(_camera);

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
            textureDraw = Content.Load<Effect>("Effects/TextureDraw");

            // create a new sound manager
            _soundManager = new SoundManager(this);

            // load in the clips we need to use
            _soundManager.registerSoundClip("select", "select");
            _soundManager.registerSoundClip("jump", "jump");
            _soundManager.registerSoundClip("menublip", "likeit");
            _soundManager.registerSoundClip("die", "death");
            _soundManager.registerSoundClip("level_complete", "levelcompleted");
            _soundManager.registerSoundClip("health_gain", "healthboost");
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

            // TODO: Add your update logic here
            beginTick();

            // check to see if this level has been completed
            int xDifference = 70;
            int yDifference = 70;
            int zDifference = 70;

            Vector3 playerPosition = _allPlayers.ElementAt(0).position;
            Vector3 goalPosition = getCurrentLevel().goal.position;

            if (Math.Abs(playerPosition.X - goalPosition.X) < xDifference && Math.Abs(playerPosition.Y - goalPosition.Y) < yDifference && Math.Abs(playerPosition.Z - goalPosition.Z) < zDifference)
            {
                this.playSoundClip("level_complete");
                levelCompleted();
            }

            // rotate the star
            float angle = MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds / 5.0f);
            getCurrentLevel().goal.rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle);
            
            // check to see if the player has died
            if (_allPlayers.ElementAt(0).toBeRemoved)
            {
                Console.WriteLine("You died.");

                //_gameOver = true; // this will triger the game over menu
                //this.paused = true;
                
            }

            // iterate over all objects and remove objects that have fallen off map
            foreach (GameObject o in _updatedObjects)
            {
                if (o.toBeRemoved && !(o is PlayerObject))
                    this.removeObject(o);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //! note there should only be one call to spritebatch begin and end for the entire menu drawing.
            //multiple draw calls can be made within here.
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //DrawTextures here

            //spriteBatch.Draw(tex, rect, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        # region Game-specific methods

        protected void LoadLevel(int level)
        {
            // load the current level
            _xmlParser = new XMLParser(getLevel(level).levelFile);

            _allObjects.Clear();
            _allTerrain.Clear();
            _allAIObjects.Clear();
            _allPlayers.Clear();

            // Load the current level
            List<GameObject> objs = new List<GameObject>();
            _xmlParser.loadLevel(objs, null);

            Random random = new Random();

            // this is the level goal
            GameObject goal = new GameObject(null, ModelType.OBJECT, ModelName.STAR, new Vector3(random.Next(0, 1000), random.Next(60, 200), random.Next(0, 1000)), Quaternion.Identity, true, new Vector3(60, 60, 60), 1000.0f, GameConstants.GameObjectScale);
            objs.Add(goal);
            getLevel(level).goal = goal;

            // this is the player
            GameObject testBox1 = new PlayerObject(null, ModelName.PLAYER, new Vector3(200, 400, 100), Quaternion.Identity, new Vector3(60, 60, 60), 1000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox1);
            testBox1.applyForce(new Vector3(-50000.0f, 0.0f, 0.0f), new Vector3(30, 30, 30));

            // Adda an AI object for testing
            GameObject testAI = new AIObject(null, ModelName.BOX, new Vector3(100, 300, 100), Quaternion.Identity, new Vector3(60, 60, 60), 10000.0f, GameConstants.GameObjectScale);
            objs.Add(testAI);

            testAI = new AIObject(null, ModelName.BOX, new Vector3(1000, 300, 1000), Quaternion.Identity, new Vector3(60, 60, 60), 9000.0f, GameConstants.GameObjectScale);
            objs.Add(testAI);

            testAI = new AIObject(null, ModelName.BOX, new Vector3(1000, 300, 100), Quaternion.Identity, new Vector3(60, 60, 60), 8000.0f, GameConstants.GameObjectScale);
            objs.Add(testAI);

            testAI = new AIObject(null, ModelName.BOX, new Vector3(100, 300, 1000), Quaternion.Identity, new Vector3(60, 60, 60), 5000.0f, GameConstants.GameObjectScale);
            objs.Add(testAI);
           
            // add a box for testing
            GameObject testBox2 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(100, 400, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox2);
            testBox2.applyForce(new Vector3(50000.0f, 0.0f, 50000.0f), new Vector3(30, 30, 30));

            // a heavier box
            GameObject testBox3 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 400, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 4000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox3);

            // an even heavier box
            GameObject testBox4 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(500, 400, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 11000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox4);

            GameObject testBox5 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 300, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox5);

            GameObject testBox6 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 400, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox6);

            //testBox1.applyForce(new Vector3(0.0f, 9.8f * testBox1.mass, 0.0f), new Vector3(60, 40, 30));

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

            _removedObjects = _removingObjects;
            _objectsRemoved = _objectsRemoving;
            _removingObjects = new List<GameObject>();

            _updatingObjects = new List<GameObject>();
            _AIChanging = false;
            _playersChanging = false;
            _playersChanging = false;
        }

        #endregion

        #region Object list modifiers

        // You must call one of these if it is necessary to add any objects to the game
        // DO NOT ADD DIRECTLY TO THE PUBLIC LISTS

        // Note: only call this for objects that are NOT of a specific subtype
        public void addGenericObject(GameObject o)
        {
            _allObjects.Add(o);
        }

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
            if(!_updatingObjects.Contains(o))
                _updatingObjects.Add(o);

            if (o is TerrainObject)
                _terrainChanging = true;
            else if (o is AIObject)
                _AIChanging = true;
            else if (o is PlayerObject)
                _playersChanging = true;
        }

        // Removes the given object from the world
        public void removeObject(GameObject o)
        {
            if (_allObjects.Contains(o))
            {
                _objectsRemoving = true;
                _removingObjects.Add(o);

                _allObjects.Remove(o);
                if (o is TerrainObject)
                    _allTerrain.Remove((TerrainObject)o);
                else if (o is PlayerObject)
                    _allPlayers.Remove((PlayerObject)o);
                else if (o is AIObject)
                    _allAIObjects.Remove((AIObject)o);
            }
        }

        #endregion

        #region Gets and sets

        public List<GameObject> allObjects { get { return _allObjects; } }
        public List<TerrainObject> allTerrain { get { return _allTerrain; } }
        public List<AIObject> allAI { get { return _allAIObjects; } }
        public List<PlayerObject> allPlayer { get { return _allPlayers; } }

        public List<GameObject> updated { get { return _updatedObjects; } }
        public List<GameObject> removed { get { return _removedObjects; } }

        #endregion

        public GameCamera camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        public GameObject topmostObject(int X, int Y)
        {
            return ((AI)_ai).topmostObject(X, Y);
        }

        public TerrainObject topmostTerrain(int X, int Y)
        {
            return ((AI) _ai).topmostTerrain(X,Y);
        }

        /*
         * This sets pause for the game.
         * */
        public Boolean paused
        {
            get { return _paused; }
            set
            {
                _paused = value;

                // show game over menu if they died
                if (_gameOver && _paused)
                {
                    ((MenuComponent)_menuComponent).showGameOverMenu(true);
                    return;
                }
                // hide it if they didn't
                ((MenuComponent)_menuComponent).showGameOverMenu(false);

                // show pause menu when appropriate
                if (_paused)
                    ((MenuComponent)_menuComponent).showPauseMenu(true);
                else
                    ((MenuComponent)_menuComponent).showPauseMenu(false);
            }
        }

        /*
         * This passes input to the menus.
         * */

        public void menuInput(int direction, Boolean select)
        {
            ((MenuComponent)_menuComponent).menuInput(direction, select);
        }

        /*
         * This method returns a level given the level number
         * */
        public LevelObject getLevel(int level)
        {
            return _levels.ElementAt(level);
        }

        /*
         * This method returns the current level.
         * */
        
        public LevelObject getCurrentLevel()
        {
            return getLevel(_currentLevel);
        }
        /*
         * This method should be called when a level is completed.  It will
         * either load the next level or run game over if all levels were
         * completed.
         * */

        public void levelCompleted()
        {
            // check if the game is over
            if (_currentLevel + 1 >= _levels.Count())
            {
                _gameOver = true;
                Console.WriteLine("Game Over!");
            }
            else
            {
                // increment the current level
                _currentLevel++;
                // load this level
                LoadLevelIndex(_currentLevel);
            }
            
        }

        /*
         * Wraps up a few methods needed to load a level after the intial level load.
         * */

        public void LoadLevelIndex(int index)
        {
            LoadLevel(index);
            ((Renderer)_renderer).setModels();
            _camera.setPlayerToFollow(_allPlayers.ElementAt(0));
            ((InputManager)_inputManager).setPlayerToControl(_allPlayers.ElementAt(0));
        }

        /*
         * This is used to play a sound clip that the sound manager contains.
         * */

        public void playSoundClip(String name)
        {
            _soundManager.playSound(name);
        }

    }
}
