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
using Muffin.Components.Renderer;
using Definitions;

namespace Muffin.Components.UI
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class InputManager : Microsoft.Xna.Framework.GameComponent
    {
        ControllerInterface _controllerOne;
        MuffinGame _muffinGame;
        KeyboardInterface _keyboard;
        GameCamera _camera;

        public InputManager(Game game)
            : base(game)
        {
            _muffinGame = (MuffinGame)game;

       
            // set up the controllers/keyboards
            _controllerOne = new ControllerInterface(_muffinGame.getPlayer(), PlayerIndex.One, _muffinGame);
            _keyboard = new KeyboardInterface(_muffinGame.allPlayer.ElementAt(0), _muffinGame);

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        /// 
        

        public override void Initialize()
        {
            _camera = _muffinGame.camera;
            
            
            base.Initialize();
            
        }

        public void setPlayerToControl(GameObject player)
        {
            _controllerOne.setPlayerToControl(player);
            _keyboard.setPlayerToControl(player);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            /*
             * Update the interfaces.  Keyboard must go first.  Controllers will override
             * the keyboard, so they must be placed second (they will only override if they
             * are turned on.  Perhaps obvious, but it was a case to consider.
             * */


            if (_controllerOne.isConnected())
            {
                _controllerOne.Update(gameTime, _camera);
            }
            else
            {
                _keyboard.Update(gameTime, _camera);
            }

            base.Update(gameTime);
        }

    }
}