using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace CS32_Group_Project
{
    public class InputHandler
    {
        private KeyboardBindings _keyboardBindings;
        private GamePadBindings _buttonBindings;
        private GamePadBindings _thumbstickBindings;
        private Game _game;

        public InputHandler(Game game)
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

            _thumbstickBindings.AddInput("left", Buttons.LeftThumbstickLeft);
            _thumbstickBindings.AddInput("right", Buttons.LeftThumbstickRight);
            _thumbstickBindings.AddInput("up", Buttons.LeftThumbstickUp);
            _thumbstickBindings.AddInput("down", Buttons.LeftThumbstickDown);
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
            return _keyboardBindings.IsUntouched(inputName) && _buttonBindings.IsUntouched(inputName) && _thumbstickBindings.IsUntouched(inputName);
        }

        public Vector3 UpdatePosition()
        {
            _keyboardBindings.UpdateInputState();
            _buttonBindings.UpdateInputState();
            _thumbstickBindings.UpdateInputState();
            if (this.IsPressed("quit"))
                _game.Exit();
            Single x = 0, y = 0, z = 0;
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
            return new Vector3(x, y, z);
        }
    }

    public enum InputState
    {
        NULL,
        PRESSED,
        RELEASED,
        UNTOUCHED,
        HELD
    }

    public abstract class InputBindings<T>
    {
        //abstract methods from which subclasses must extend
        public abstract void UpdateInputState();
        public abstract InputState GetState(String inputName);

        protected Dictionary<String, T> _inputHashTable;

        public InputBindings()
        {
            _inputHashTable = new Dictionary<String, T>();
        }

        public void AddInput(String inputName, T input)
        {
            if (_inputHashTable.ContainsKey(inputName))
                _inputHashTable.Add(inputName, input);
            else
                _inputHashTable.Add(inputName, input);
        }

        //exceptions? check to see if hashTable contains key?
        public T GetInput(String inputName)
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
                Keys boundKey = this.GetInput(inputName);
                if (currentState.IsKeyDown(boundKey))
                    keyState = InputState.PRESSED;
                else if (oldState.IsKeyDown(boundKey))
                    keyState = InputState.RELEASED;
                else
                    keyState = InputState.UNTOUCHED;
                if (currentState.IsKeyDown(boundKey) && oldState.IsKeyDown(boundKey))
                    keyState = InputState.HELD;
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
                Buttons boundButton = this.GetInput(inputName);
                if (currentState.IsButtonDown(boundButton))
                    buttonState = InputState.PRESSED;
                else if (oldState.IsButtonDown(boundButton))
                    buttonState = InputState.RELEASED;
                else
                    buttonState = InputState.UNTOUCHED;
                if (currentState.IsButtonDown(boundButton) && oldState.IsButtonDown(boundButton))
                    buttonState = InputState.HELD;
            }
            return buttonState;
        }
    }
}
