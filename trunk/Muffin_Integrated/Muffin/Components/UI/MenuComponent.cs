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


namespace Muffin.Components.UI
{
    /*
     * This delegate is used for passing methods into
     * the menu, for reactions to save, load, etc.
     * */

    public delegate void menuCallback();

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MenuComponent : DrawableGameComponent
    {
        private MuffinGame _muffinGame;
        private SpriteBatch _spriteBatch;
        private MenuObject _pauseMenu, _mainMenu;
        private InputManager _inputManager;

        public MenuComponent(Game game, InputManager inputManager)
            : base(game)
        {
            
            _muffinGame = (MuffinGame)game;
            _inputManager = inputManager;
            
            
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // make a new spritebatch
            _spriteBatch = new SpriteBatch(_muffinGame.GraphicsDevice);

            // make our menus (content added below)
            _pauseMenu = new MenuObject(_spriteBatch, _muffinGame);
            _mainMenu = new MenuObject(_spriteBatch, _muffinGame);

            base.Initialize();
        }

        protected override void LoadContent()
        {

            double height = _muffinGame.graphics.PreferredBackBufferHeight;
            double width = _muffinGame.graphics.PreferredBackBufferWidth;
            
            // load all of the images for the menu
            _pauseMenu.addItem("pauseMenuBackground", new Rectangle(738, 147, 444, 907), false, null);
            _pauseMenu.addItem("pauseMenuSave", new Rectangle(765, 210, 750, 70), true, new menuCallback(save));
            _pauseMenu.addItem("pauseMenuLoad", new Rectangle(770, 300, 769, 90), true, new menuCallback(load));
            _pauseMenu.addItem("pauseMenuOptions", new Rectangle(765, 400, 1118, 109), true, new menuCallback(options));
            _pauseMenu.addItem("pauseMenuMain", new Rectangle(770, 500, 818, 91), true, new menuCallback(main));
            _pauseMenu.addItem("pauseMenuQuit", new Rectangle(765, 600, 1089, 109), true, new menuCallback(quit));
            _pauseMenu.addItem("pauseMenuResume", new Rectangle(770, 720, 959, 70), true, new menuCallback(resume));

            
            base.LoadContent();
        }

        #region callbacks
        /*
         * These methods are all of the callbacks for menu items.
         * */

        public void resume()
        {
            _muffinGame.paused = false;
        }

        public void save()
        {
            Console.WriteLine("Saving would be implemented here.");
        }

        public void load()
        {
            Console.WriteLine("Loading would be implemented here.");
            _muffinGame.levelCompleted();
        }

        public void options()
        {
            Console.WriteLine("Options would be implemented here.");
        }

        public void main()
        {
            Console.WriteLine("Main would be implemented here.");
        }

        public void quit()
        {
            Environment.Exit(0);   
        }

        #endregion

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            _pauseMenu.draw();
            _spriteBatch.End();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // check input
           

            base.Update(gameTime);
        }

        public void showPauseMenu(Boolean show)
        {
            _pauseMenu.hidden = !show;
        }

        /*
         * This method passes input to the menus.  Input will only be handled if the menu
         * is active, so there is no need to do any checking.
         * */
        
        public void menuInput(int direction, Boolean select)
        {
            _pauseMenu.menuInput(direction, select);
            _mainMenu.menuInput(direction, select);
        }

        

    }
}