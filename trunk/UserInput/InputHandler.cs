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

namespace _3D_Renderer
{
    public class InputHandler : GameComponent
    {
        private KeyboardBindings _keyboardBindings;
        private GamePadBindings _buttonBindings;
        private GamePadBindings _thumbstickBindings;
        private Renderer _game;
        private bool _lockMenuInput;

        //add an input binding here!
        public InputHandler(Renderer game) : base(game)
        {
            _game = game;
            _keyboardBindings = new KeyboardBindings();
            _buttonBindings = new GamePadBindings();
            _thumbstickBindings = new GamePadBindings();

            _keyboardBindings.AddInput("quit", Keys.Escape);

            _keyboardBindings.AddInput("left", Keys.Left);
            _keyboardBindings.AddInput("right", Keys.Right);
            _keyboardBindings.AddInput("up", Keys.Up);
            _keyboardBindings.AddInput("down", Keys.Down);

            _buttonBindings.AddInput("left", Buttons.DPadLeft);
            _buttonBindings.AddInput("right", Buttons.DPadRight);
            _buttonBindings.AddInput("up", Buttons.DPadUp);
            _buttonBindings.AddInput("down", Buttons.DPadDown);
            _buttonBindings.AddInput("start", Buttons.Start);

            _thumbstickBindings.AddInput("left", Buttons.LeftThumbstickLeft);
            _thumbstickBindings.AddInput("right", Buttons.LeftThumbstickRight);
            _thumbstickBindings.AddInput("up", Buttons.LeftThumbstickUp);
            _thumbstickBindings.AddInput("down", Buttons.LeftThumbstickDown);

            _lockMenuInput = false;
        }

        public bool IsPressed(String inputName)
        {
            return _keyboardBindings.IsPressed(inputName) || _buttonBindings.IsPressed(inputName) || _thumbstickBindings.IsPressed(inputName);
        }

        public bool IsReleased(String inputName)
        {
            return _keyboardBindings.IsReleased(inputName) || _buttonBindings.IsReleased(inputName) || _thumbstickBindings.IsReleased(inputName);
        }

        public bool IsHeld(String inputName)
        {
            return _keyboardBindings.IsHeld(inputName) || _buttonBindings.IsHeld(inputName) || _thumbstickBindings.IsHeld(inputName);
        }

        public bool IsUntouched(String inputName)
        {
            return (_buttonBindings.GetState(inputName) == InputState.NULL) || (_buttonBindings.GetState(inputName) == InputState.UNTOUCHED);
            //do the same for _buttonBindings and _thumbstickBindings
        }

        /* for muli-player, implement above methods by calling:
        public bool checkBindings(String inputName, InputState inputState)
        {
            bool found = false;
            for (int i = 0; i < _allBindings.size(); i++)
            {
                if (_allBindings.get(i).GetState(inputName) == inputState)
                    found = true;
            }
            return found;
        }
         */

        public Vector3 UpdatePosition()
        {
            if (this.IsPressed("quit"))
                _game.Exit();
            Single x = 0, y = 0, z = 0;
            /*
//if the key is held:
if (this.IsHeld("left"))
    x = -100;
if (this.IsHeld("right"))
    x = 100;
if (this.IsHeld("left") && this.IsHeld("right"))
    x = 0;
if (this.IsHeld("up"))
    y = 100;
if (this.IsHeld("down"))
    y = -100;
if (this.IsHeld("up") && this.IsHeld("down"))
    y = 0;
//define actions for IsPressed if needed
 */
            return new Vector3(x, y, z);
        }
        public override void Initialize()
        {
            // TODO: Add your initialization logic here

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            _keyboardBindings.UpdateGameTime(gameTime);
            _buttonBindings.UpdateGameTime(gameTime);
            _thumbstickBindings.UpdateGameTime(gameTime);
            _keyboardBindings.UpdateInputState();
            _buttonBindings.UpdateInputState();
            _thumbstickBindings.UpdateInputState();
            if (this.IsHeld("start") && _buttonBindings.GetInput("start")._timeHeld > 150 && !_lockMenuInput)
            {
                if (_game._showMenu == false && !_lockMenuInput)
                {
                    _game.SetMenuVisible(true);
                    _lockMenuInput = true;
                }
                else if (_game._showMenu == true && !_lockMenuInput)
                {
                    _game.SetMenuVisible(false);
                    _lockMenuInput = true;
                }
            }
            if (this.IsUntouched("start"))
            {
                Console.WriteLine("Nobody is touching me =O");
                _lockMenuInput = false;
            }

            base.Update(gameTime);
        }

    }
}
