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
    public abstract class InputBindings<T>
    {
        public int _elapsedTime = 0;

        //abstract methods from which subclasses must extend
        public abstract void UpdateInputState();
        public abstract InputState GetState(String inputName);

        protected Dictionary<String, InputBinding<T>> _inputHashTable;

        public void UpdateGameTime(GameTime gameTime)
        {
            _elapsedTime = gameTime.ElapsedGameTime.Milliseconds;
        }

        public InputBindings()
        {
            _inputHashTable = new Dictionary<String, InputBinding<T>>();
        }

        public void AddInput(String inputName, T input)
        {
            if (!_inputHashTable.ContainsKey(inputName))
                _inputHashTable.Add(inputName, new InputBinding<T>(input));
        }

        //exceptions? check to see if hashTable contains key?
        public InputBinding<T> GetInput(String inputName)
        {
            return _inputHashTable[inputName];
        }

        public bool IsPressed(String inputName)
        {
            return this.GetState(inputName) == InputState.PRESSED;
        }

        public bool IsReleased(String inputName)
        {
            return this.GetState(inputName) == InputState.RELEASED;
        }

        public bool IsUntouched(String inputName)
        {
            return this.GetState(inputName) == InputState.UNTOUCHED;
        }

        public bool IsHeld(String inputName)
        {
            return this.GetState(inputName) == InputState.HELD;
        }
    }

    public class KeyboardBindings : InputBindings<Keys>
    {
        private KeyboardState oldState;
        private KeyboardState currentState;

        public KeyboardBindings()
        {
            oldState = Keyboard.GetState();
            currentState = Keyboard.GetState();
        }

        public override void UpdateInputState()
        {
            oldState = currentState;
            currentState = Keyboard.GetState();
        }

        public override InputState GetState(String inputName)
        {
            InputState keyState = InputState.NULL;
            if (_inputHashTable.ContainsKey(inputName))
            {
                InputBinding<Keys> boundInput = this.GetInput(inputName);
                Keys boundKey = boundInput._input;
                if (currentState.IsKeyDown(boundKey))
                    keyState = InputState.PRESSED;
                else if (oldState.IsKeyDown(boundKey))
                    keyState = InputState.RELEASED;
                else
                    keyState = InputState.UNTOUCHED;
                if (currentState.IsKeyDown(boundKey) && oldState.IsKeyDown(boundKey))
                {
                    keyState = InputState.HELD;
                    boundInput._timeHeld += _elapsedTime;
                }
                else
                    boundInput._timeHeld = 0;
            }
            return keyState;
        }
    }

    public class GamePadBindings : InputBindings<Buttons>
    {
        private GamePadState oldState;
        private GamePadState currentState;

        public GamePadBindings()
        {
            oldState = GamePad.GetState(PlayerIndex.One);
            currentState = GamePad.GetState(PlayerIndex.One);
        }

        public override void UpdateInputState()
        {
            oldState = currentState;
            currentState = GamePad.GetState(PlayerIndex.One);
        }

        public override InputState GetState(String inputName)
        {
            InputState buttonState = InputState.NULL;
            if (_inputHashTable.ContainsKey(inputName))
            {
                InputBinding<Buttons> boundInput = this.GetInput(inputName);
                Buttons boundButton = boundInput._input;
                if (currentState.IsButtonDown(boundButton))
                    buttonState = InputState.PRESSED;
                else if (oldState.IsButtonDown(boundButton))
                    buttonState = InputState.RELEASED;
                else
                    buttonState = InputState.UNTOUCHED;
                if (currentState.IsButtonDown(boundButton) && oldState.IsButtonDown(boundButton))
                {
                    buttonState = InputState.HELD;
                    boundInput._timeHeld += _elapsedTime;
                }
                else
                    boundInput._timeHeld = 0;
            }
            return buttonState;
        }
    }
}
