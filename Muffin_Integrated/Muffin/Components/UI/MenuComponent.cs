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

    /*
     * Enums for indexing sound clips.
     * */

    public enum SoundClip : int
    {
        CHANGE = 0,
        SELECT = 1
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MenuComponent : DrawableGameComponent
    {
        private MuffinGame _muffinGame;
        private SpriteBatch _spriteBatch;
        private MenuObject _pauseMenu, _mainMenu, _gameOverMenu, _levelCompleteMenu, _levelFailedMenu;

        

        public MenuComponent(Game game)
            : base(game)
        {
            
            _muffinGame = (MuffinGame)game;
            
            
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // make a new spritebatch
            _spriteBatch = new SpriteBatch(_muffinGame.GraphicsDevice);

           
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // make our menus (content added below)
            _pauseMenu = new MenuObject(_spriteBatch, _muffinGame);
            _pauseMenu.addItem("pauseMenu/pauseMenuBackground", new Rectangle(652, 370, 615, 459), false, null);
            _pauseMenu.addItem("pauseMenu/restart", new Rectangle(652, 370, 615, 459), true, new menuCallback(restart));
            _pauseMenu.addItem("pauseMenu/mainPause", new Rectangle(652, 370, 615, 459), true, new menuCallback(main));
            _pauseMenu.addItem("pauseMenu/resume", new Rectangle(652, 370, 615, 459), true, new menuCallback(resume));

            // main menu
            _mainMenu = new MenuObject(_spriteBatch, _muffinGame);
            _mainMenu.hidden = false;

            // load main menu components
            _mainMenu.addItem("mainMenu", new Rectangle(0, 0, 1920, 1200), false, null);
            _mainMenu.addItem("healthBar", new Rectangle(1250, 40, 300, 50), false, null);

            // game over menu
            _gameOverMenu = new MenuObject(_spriteBatch, _muffinGame);
            _gameOverMenu.addItem("gameOverMenu/win", new Rectangle(526, 517, 868, 166), false, null);
            _gameOverMenu.addItem("gameOverMenu/congrats", new Rectangle(520, 527, 868, 166), true, new menuCallback(gameOver));

            // level completed menu

            _levelCompleteMenu = new MenuObject(_spriteBatch, _muffinGame);
            _levelCompleteMenu.addItem("levelCompletedMenu/complete", new Rectangle(727, 459, 467, 282), false, null);
            _levelCompleteMenu.addItem("levelCompletedMenu/next", new Rectangle(727, 459, 467, 282), true, new menuCallback(nextLevel));
            _levelCompleteMenu.addItem("levelCompletedMenu/mainComplete", new Rectangle(727, 459, 467, 282), true, new menuCallback(main));



            // level failed menu
            _levelFailedMenu = new MenuObject(_spriteBatch, _muffinGame);
            _levelFailedMenu.addItem("levelFailedMenu/death", new Rectangle(727, 462, 465, 276), false, null);
            _levelFailedMenu.addItem("levelFailedMenu/retry", new Rectangle(727, 462, 465, 276), true, new menuCallback(retry));
            _levelFailedMenu.addItem("levelFailedMenu/mainDeath", new Rectangle(727, 462, 465, 276), true, new menuCallback(main));
            base.LoadContent();
        }

        #region callbacks
        /*
         * These methods are all of the callbacks for menu items.
         * */

        /*
         * This is used for level complted.
         * */

        public void nextLevel()
        {
            _muffinGame.displayLevelComplete(false);
            
        }
        /*
         * This is used for the pause menu.
         * */
        public void restart()
        {
            _muffinGame.retryLevel();
            _muffinGame.togglePauseMenu();
        }
        /*
         * This is used at the end of failing a level.
         * */

        public void retry()
        {
            _muffinGame.retryLevel();
            _muffinGame.displayLevelFailed(false);

        }
        /*
         * This is used to resume from pause.
         * */

        public void resume()
        {
            _muffinGame.togglePauseMenu();
        }

        public void main()
        {
            Console.WriteLine("Main would be implemented here.");
            this.quit();
        }

        public void quit()
        {
            Environment.Exit(0);
        }

        public void gameOver()
        {
            _muffinGame.newGame();
            _muffinGame.displayGameOver(false);
        }

        #endregion

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            _pauseMenu.draw();
            _mainMenu.draw();
            _gameOverMenu.draw();
            _levelCompleteMenu.draw();
            _levelFailedMenu.draw();
            //this.decrementHealth();
            this.drawTime(gameTime);
            _spriteBatch.End();
        }

        public void drawTime(GameTime gameTime)
        {
            string output = gameTime.TotalGameTime.Seconds.ToString();
            SpriteFont Font1 = _muffinGame.Content.Load<SpriteFont>("Courier New");
            Vector2 FontPos = new Vector2(_muffinGame.GraphicsDevice.Viewport.Width / 2,
            _muffinGame.GraphicsDevice.Viewport.Height / 2);
            Vector2 FontOrigin = Font1.MeasureString(output) / 2;
            // Draw the string
            _spriteBatch.DrawString(Font1, output, new Vector2(1350, 70), Color.White,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }

        int counter = 200;
        public void decrementHealth()
        {
            counter--;
            //_mainMenu.addItem("healthBar", new Rectangle(set dimensions), false, null);
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

        public void showGameOverMenu(Boolean show)
        {
            _gameOverMenu.hidden = !show;
        }

        public void showLevelCompleteMenu(Boolean show)
        {

            _levelCompleteMenu.hidden = !show;

        }

        public void showLevelFailedMenu(Boolean show)
        {
            _levelFailedMenu.hidden = !show;
        }

        /*
         * This method passes input to the menus.  Input will only be handled if the menu
         * is active, so there is no need to do any checking.
         * */
        
        public void menuInput(int direction, Boolean select)
        {
            _pauseMenu.menuInput(direction, select);
            _mainMenu.menuInput(direction, select);
            _gameOverMenu.menuInput(direction, select);
            _levelCompleteMenu.menuInput(direction, select);
            _levelFailedMenu.menuInput(direction, select);
        }

        

    }
}