using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.IO;
using System.Text;

namespace TextBasedLevelEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1280;
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 
        private MouseHandler _mouseHandler;
        private SpriteBatch _spriteBatch;
        private Level _level;
        private int _currentLayer;
        private SelectedObject _selectedObject;
        private Menu _menu;
        private KeyboardHandler _keyboardHandler;
        private XMLSave _xml;
        private _3DPreview _preview, _previewCut;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _mouseHandler = new MouseHandler(0, 0, this);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _level = new Level(Constants.gridSizeX, Constants.gridSizeY, Constants.gridSizeZ);
            _currentLayer = 0;
            _selectedObject = new SelectedObject("grass", this, Vector3.Zero, null);
            _menu = new Menu(700, 0, this);
            _keyboardHandler = new KeyboardHandler();
            _xml = new XMLSave();
            _preview = new _3DPreview(this, 900, 500);
            _previewCut = new _3DPreview(this, 900, 200);

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
            
            Console.WriteLine("Level " + (_currentLayer + 1));
            // TODO: use this.Content to load your game content here
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

            Vector2 position = _mouseHandler.getGridPosition() * Constants.gridSize;
            
            //  update the position of the selected object
            _selectedObject.updatePosition(position);
            _menu.updateSelected();
            _keyboardHandler.update();

            if (_keyboardHandler.keyPressed(Keys.Up))
            {
                if (_currentLayer + 1 < Constants.gridSizeY)
                    _currentLayer++;

                Console.WriteLine("Level " + (_currentLayer + 1));
            }

            else if (_keyboardHandler.keyPressed(Keys.Down))
            {
                if (_currentLayer - 1 >= 0)
                    _currentLayer--;

                Console.WriteLine("Level " + (_currentLayer + 1));
            }

            if (_keyboardHandler.keyPressed(Keys.S))
                _xml.write(_level);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            _level.drawLayer(_currentLayer, _spriteBatch);
            _selectedObject.draw(_spriteBatch);
            _menu.draw(_spriteBatch);
            _preview.draw(_level, _spriteBatch);
            _previewCut.draw(_level, _spriteBatch, _currentLayer + 1);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        /*
         * These methods deal with adding objects to the grid.
         * */

        public void addSelectedObjectAtPosition(Vector2 position)
        {
            
            if(_level.registerObject(new LevelObject(_selectedObject.modelName,Constants.objectSize * (new Vector3(position.X, _currentLayer / 2.0f, position.Y)), this.getSize(_selectedObject.modelName), 0.0f, this)))
                Console.WriteLine("Added a " + _selectedObject.modelName + " tile.");
        }

        public void removeObjectAtPosition(Vector2 position)
        {
            if (_level.removeObject((int)position.X, _currentLayer, (int)position.Y))
                Console.WriteLine("Deleted!");
        }

        /*
         * This is a type checked remove--only objects of the currently selected type will be deleted.
         * */

        public void removeObjectOfTypeAtPosition(Vector2 position)
        {
            if (_level.removeObjectOfType((int)position.X, _currentLayer, (int)position.Y,_selectedObject.modelName))
                Console.WriteLine("Deleted!");
        }

        public Vector3 getSize(String modelName)
        {
            switch (modelName)
            {
                case "grass":
                    return new Vector3(60, 30, 60);
                
                default:
                    return new Vector3(60, 60, 60);
                    
            }
        }

        public void updateSelectedObject(String modelName)
        {
            _selectedObject = new SelectedObject(modelName, this, _selectedObject.position, null);
        }
    }
}
