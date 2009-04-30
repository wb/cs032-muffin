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
        private List<CollectableObject> _allCollectables;

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

        private Boolean _pauseMenu = false, _levelCompleteMenu = false, _gameOverMenu = false, _levelFailedMenu = false;

        private Effect textureDraw;

        private int _numberOfCoins = 0;

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
            _allCollectables = new List<CollectableObject>();

            _updatedObjects = new List<GameObject>();
            _updatingObjects = new List<GameObject>();

            _removedObjects = new List<GameObject>();
            _removingObjects = new List<GameObject>();

            // create some levels
            _levels = new List<LevelObject>();
            _levels.Add(new LevelObject("level"));
            _levels.Add(new LevelObject("level_ahh"));
            _levels.Add(new LevelObject("level_new_player_ai_coins_star"));
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
            _camera = new GameCamera(this.getPlayer(), 24 * new Vector3(-20, 40, -20), new Vector3(0, 0, 0), graphics.GraphicsDevice.Viewport.AspectRatio);
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

            // create a new sound manager
            _soundManager = new SoundManager(this);

            // load in the clips we need to use
            _soundManager.registerSoundClip("select", "select");
            _soundManager.registerSoundClip("jump", "jump");
            _soundManager.registerSoundClip("menublip", "likeit");
            _soundManager.registerSoundClip("die", "death");
            _soundManager.registerSoundClip("level_complete", "levelcompleted");
            _soundManager.registerSoundClip("health_gain", "healthboost");
            _soundManager.registerSoundClip("star_born", "star_born");
            _soundManager.registerSoundClip("coinCollected", "coin");

            // play background music
            MediaPlayer.Play(Content.Load<Song>("Audio\\background"));
            MediaPlayer.IsRepeating = true;
                        
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

            // loop through all collectables
            foreach (CollectableObject o in _allCollectables)
            {
                o.updateOrientation(gameTime);
                o.checkForCollection(this);
            }

            // only show the star when all coins have been collected
            if (this.getPlayer().coinCount == _numberOfCoins && _numberOfCoins > 0)
            {
                if (getCurrentLevel().goal.hidden == true)
                {
                    this.playSoundClip("star_born");
                    getCurrentLevel().goal.hidden = false;
                }
            }

            // check to see if the player has died and restart the level if so
            if (this.getPlayer().toBeRemoved)
                this.displayLevelFailed(true);

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

            spriteBatch.End();

            base.Draw(gameTime);
        }

        # region Game-specific methods
        
        protected void LoadLevel(int level)
        {
            // load the current level
            _xmlParser = new XMLParser(getLevel(level).levelFile, this);

            _allObjects.Clear();
            _allTerrain.Clear();
            _allAIObjects.Clear();
            _allPlayers.Clear();

            // Load the current level
            List<GameObject> objs = new List<GameObject>();
            _xmlParser.loadLevel(objs, null);

            
            
            #region testing objects
            /*
            // coins
            for (int i = 0; i < 12; i++)
            {
                int x = (int) random.Next(0, maxX);
                int z = (int) random.Next(0, maxZ);
                int y = 60;
                objs.Add(new CollectableObject(ModelName.COIN, new CollectionCallback(coinCollected), player, 40.0f, new Vector3(x, y, z), new Vector3(60, 60, 60), GameConstants.GameObjectScale, false));
            }
            // create a new goal
            int xPosition = (int)random.Next(0, maxX);
            int zPosition = (int)random.Next(0, maxZ);
            GameObject goal = new CollectableObject(ModelName.STAR, new CollectionCallback(starCollected), player, 30.0f, new Vector3(xPosition, random.Next(60, 200), zPosition), new Vector3(60, 60, 60), GameConstants.GameObjectScale, true);
            objs.Add(goal);
            getLevel(level).goal = (goal as CollectableObject);

            // ai objects
            objs.Add(new AIObject(null, ModelName.BOX, new Vector3(100, 300, 100), Quaternion.Identity, new Vector3(60, 60, 60), 10000.0f, GameConstants.GameObjectScale));
            objs.Add(new AIObject(null, ModelName.BOX, new Vector3(1000, 300, 1000), Quaternion.Identity, new Vector3(60, 60, 60), 9000.0f, GameConstants.GameObjectScale));
            objs.Add(new AIObject(null, ModelName.BOX, new Vector3(1000, 300, 100), Quaternion.Identity, new Vector3(60, 60, 60), 8000.0f, GameConstants.GameObjectScale));
            objs.Add(new AIObject(null, ModelName.BOX, new Vector3(100, 300, 1000), Quaternion.Identity, new Vector3(60, 60, 60), 5000.0f, GameConstants.GameObjectScale));
           
            // add a box for testing
            GameObject testBox2 = new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(100, 400, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale);
            objs.Add(testBox2);
            testBox2.applyForce(new Vector3(50000.0f, 0.0f, 50000.0f), new Vector3(30, 30, 30));

            // four more boxes of varying masses
            objs.Add(new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 400, 100), Quaternion.Identity, false, new Vector3(60, 60, 60), 4000.0f, GameConstants.GameObjectScale));
            objs.Add(new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(500, 400, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 11000.0f, GameConstants.GameObjectScale));
            objs.Add(new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 300, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale));
            objs.Add(new GameObject(null, ModelType.OBJECT, ModelName.BOX, new Vector3(300, 400, 500), Quaternion.Identity, false, new Vector3(60, 60, 60), 2000.0f, GameConstants.GameObjectScale));
            */
            #endregion
            foreach (GameObject o in objs)
            {
               
                

                if (o is TerrainObject)
                    addTerrainObject(o as TerrainObject);
                else if (o is AIObject)
                    addAIObject(o as AIObject);
                else if (o is PlayerObject)
                    addPlayerObject(o as PlayerObject);
                else if (o is CollectableObject)
                {
                    addCollectableObject(o as CollectableObject);
                    (o as CollectableObject).collectionObject = this.getPlayer();
                    if ((o as CollectableObject).modelName == ModelName.COIN)
                        _numberOfCoins++;
                    else if ((o as CollectableObject).modelName == ModelName.STAR)
                        this.getCurrentLevel().goal = o as CollectableObject;
                }
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

        public void addCollectableObject(CollectableObject o)
        {
            _allObjects.Add(o);
            _allCollectables.Add(o as CollectableObject);
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
                else if (o is CollectableObject)
                    _allCollectables.Remove((CollectableObject)o);
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

        #region camera stuff
        public GameCamera camera
        {
            get { return _camera; }
            set { _camera = value; }
        }
        #endregion

        #region grid stuff
        public GameObject topmostObject(int X, int Y)
        {
            return ((AI)_ai).topmostObject(X, Y);
        }

        public TerrainObject topmostTerrain(int X, int Y)
        {
            return ((AI) _ai).topmostTerrain(X,Y);
        }
        #endregion

        #region menu controls
        /*
         * This sets pause for the game.
         * */
        public Boolean paused
        {
            get { return _paused; }
            set { _paused = value; }
        }

        

        /*
         * This method shows the pause menu.
         * */
        public void togglePauseMenu()
        {
            if (_levelCompleteMenu || _levelFailedMenu || _gameOverMenu)
                return;

            _pauseMenu = !_pauseMenu;
            _paused = _pauseMenu;
            ((MenuComponent)_menuComponent).showPauseMenu(_pauseMenu);

        }

        /*
         * This method shows the level completed menu.
         * */

        public void displayLevelComplete(Boolean show)
        {
            if (show && !_levelCompleteMenu)
            {

                this.playSoundClip("level_complete");
                _levelCompleteMenu = true;
                ((MenuComponent)_menuComponent).showLevelCompleteMenu(true);
                _paused = true;

            }
            else if (!show && _levelCompleteMenu)
            {

                levelCompleted();
                _paused = false;
                _levelCompleteMenu = false;
                ((MenuComponent)_menuComponent).showLevelCompleteMenu(false);
            }

        }

        /*
         * This method shows the game over screen (you beat the game)
         * */

        public void displayGameOver(Boolean show)
        {
            if (show && !_gameOverMenu)
            {
                _paused = true;
                _gameOver = true;
                ((MenuComponent)_menuComponent).showGameOverMenu(true);
                _gameOverMenu = !_gameOverMenu;
            }
            else if (!show && _gameOverMenu)
            {
                _paused = false;
                _gameOver = false;
                ((MenuComponent)_menuComponent).showGameOverMenu(false);
                _gameOverMenu = !_gameOverMenu;
            }
        }

        /*
         * This menu shows the level failed (you died) menu.
         * */

        public void displayLevelFailed(Boolean show)
        {

            // if we are hiding it, then retry this level
            if (!show && _levelFailedMenu)
            {
                this.retryLevel();
                _levelFailedMenu = false;
                ((MenuComponent)_menuComponent).showLevelFailedMenu(false);
                _paused = false;
            }
            else if(show && !_levelFailedMenu)
            {
                Console.WriteLine("You died! Retrying level.");
                this.playSoundClip("die");
                _levelFailedMenu = true;
                ((MenuComponent)_menuComponent).showLevelFailedMenu(true);
                _paused = true;
            }

        }
        

        /*
         * This passes input to the menus.
         * */

        public void menuInput(int direction, Boolean select)
        {
            ((MenuComponent)_menuComponent).menuInput(direction, select);
        }

        #endregion

        #region level loading
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
                this.displayGameOver(true);
            else
            {
                // increment the current level
                _currentLevel++;
                // load this level
                LoadLevelIndex(_currentLevel);
            }
            
        }

        /*
         * This retries the current level.
         * */

        public void retryLevel()
        {
            LoadLevelIndex(_currentLevel);
        }

        /*
         * Wraps up a few methods needed to load a level after the intial level load.
         * */

        public void LoadLevelIndex(int index)
        {
            _numberOfCoins = 0;
            LoadLevel(index);
            ((Renderer)_renderer).setModels();
            _camera.setPlayerToFollow(this.getPlayer());
            ((InputManager)_inputManager).setPlayerToControl(this.getPlayer());
        }

        public void newGame()
        {
            _currentLevel = 0;
            this.retryLevel();
        }

        #endregion

        #region sound stuff
        /*
         * This is used to play a sound clip that the sound manager contains.
         * */

        public void playSoundClip(String name)
        {
            if (_soundManager != null)
            _soundManager.playSound(name);
        }
        #endregion

        #region collectable callbacks
        // function called when a coin is collected
        public void coinCollected()
        {
            this.getPlayer().coinCollected();
            this.playSoundClip("coinCollected");
            Console.WriteLine("You have collected " + this.getPlayer().coinCount + " of " + _numberOfCoins + " coins!");

            // set jump count to 0 (for some fun levels where coins can bridge you across
            this.getPlayer().jumpCount = 0;

        }

        // function called when a star is collected
        public void starCollected()
        {
            if (_currentLevel + 1 < _levels.Count())
                this.displayLevelComplete(true);
            else
                this.displayGameOver(true);
        }
        #endregion

        // gets the player. we can change this later, but it works for now
        public PlayerObject getPlayer()
        {
            if (_allPlayers.Count() == 0)
            {
                Console.WriteLine("Error: Level must include a player.");
                Environment.Exit(1);
            }
            return _allPlayers.ElementAt(0);
        }
    }
}
