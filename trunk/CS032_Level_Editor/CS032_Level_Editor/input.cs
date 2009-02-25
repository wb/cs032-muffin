/*
 * An Input-System that binds logical names to physical input device controls.
 * Written in 2008 by Peter "craaash" Schraut, http://www.console-dev.de
 */
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CS032_Level_Editor
{
    /// <summary>
    /// Holds information about a binded physical input control.
    /// </summary>
    public struct InputBindingState
    {
        internal bool _released;
        internal bool _pressed;
        internal bool _clicked;
        internal float _value;
        internal float _pressedMilliseconds;
        internal float _releasedMilliseconds;
        internal float _clickedMilliseconds;
        internal bool _connected;

        /// <summary>
        /// Gets an empty InputBindingState.
        /// </summary>
        static public InputBindingState Empty
        {
            get
            {
                InputBindingState state = new InputBindingState();
                state._released = true;
                state._pressed = false;
                state._clicked = false;
                state._value = 0.0f;
                state._releasedMilliseconds = float.MaxValue;
                state._pressedMilliseconds = float.MaxValue;
                state._clickedMilliseconds = float.MaxValue;
                state._connected = false;

                return state;
            }
        }

        /// <summary>
        /// Gets if the binded input control is released.
        /// </summary>
        public bool Released
        {
            get { return _released; }
        }

        /// <summary>
        /// How many milliseconds elapsed since the binded input control has been released.
        /// </summary>
        public float ReleasedMilliseconds
        {
            get { return _releasedMilliseconds; }
        }

        /// <summary>
        /// Gets if the binded input control is pressed.
        /// </summary>
        public bool Pressed
        {
            get { return _pressed; }
        }

        /// <summary>
        /// How many milliseconds elapsed since the binded input control has been pressed.
        /// </summary>
        public float PressedMilliseconds
        {
            get { return _pressedMilliseconds; }
        }

        /// <summary>
        /// Gets if the binded input control has been clicked.
        /// </summary>
        public bool Clicked
        {
            get { return _clicked; }
        }

        /// <summary>
        /// How many milliseconds elapsed since the binded input control has been clicked.
        /// </summary>
        public float ClickedMilliseconds
        {
            get { return _clickedMilliseconds; }
        }

        /// <summary>
        /// Gets a freely interpretable float value representing the current state. For
        /// digital buttons this should be 0=released, 1=pressed. Analog controls should use 0..1.
        /// </summary>
        public float Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets if the device of the binded input control is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _connected; }
        }
    }

    /// <summary>
    /// Represents the base class of an input binding.
    /// An input binding is an object that connects a name with a physical input control
    /// such as a key on the keyboard or a gamepad button and is able to return
    /// an InputBindingState of this particular control.
    /// </summary>
    public abstract class InputBinding
    {
        protected string _name = "n/a";
        protected PlayerIndex _playerIndex = PlayerIndex.One;
        protected InputBindingState _state;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <param name="name">Binding name, this is the name that you use to query its state afterwards.</param>
        public InputBinding(PlayerIndex playerIndex, string name)
        {
            _name = name;
        }

        /// <summary>
        /// Must be called every game tick to update the state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="value">Specifies the binded input control value. 0 indicates not pressed.</param>
        protected void UpdateState(GameTime gameTime, bool connected, float value)
        {
            _state._pressed = Math.Abs(value) > float.Epsilon;
            _state._clicked = _state._released && _state._pressed;
            _state._released = !_state._pressed;
            _state._value = value;
            _state._pressedMilliseconds = _state._pressed ? _state._pressedMilliseconds + (float)gameTime.ElapsedGameTime.TotalMilliseconds : 0.0f;
            _state._releasedMilliseconds = _state._released ? _state._releasedMilliseconds + (float)gameTime.ElapsedGameTime.TotalMilliseconds : 0.0f;
            _state._clickedMilliseconds = _state._clicked ? 0 : _state._clickedMilliseconds + (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _state._connected = connected;
        }

        /// <summary>
        /// Gets name of binding.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets PlayerIndex of binding.
        /// </summary>
        public PlayerIndex PlayerIndex
        {
            get { return _playerIndex; }
        }

        /// <summary>
        /// Gets state of binding.
        /// </summary>
        /// <param name="invalidate">Specify true to clear the current state, false otherwise</param>
        /// <returns>Returns a snapshot of the current binding state.</returns>
        public abstract InputBindingState GetState(bool invalidate);
    }

    /// <summary>
    /// A class that binds keys on a keyboard device to a name.
    /// </summary>
    public class KeyboardInputBinding : InputBinding
    {
        Keys _physicalKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <param name="name">Name that you use to query the state once it has been registered at the InputBindingAccess.</param>
        /// <param name="physicalKey">Key you want to bind.</param>
        public KeyboardInputBinding(PlayerIndex playerIndex, string name, Keys physicalKey)
            : base(playerIndex, name)
        {
            _physicalKey = physicalKey;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateState(gameTime, true, Keyboard.GetState(PlayerIndex).IsKeyDown(_physicalKey) ? 1 : 0);
        }

        public override InputBindingState GetState(bool invalidate)
        {
            InputBindingState state = _state;

            if (invalidate)
                _state = InputBindingState.Empty;

            return state;
        }
    }

    /// <summary>
    /// A class that binds digital gamepad buttons to a name.
    /// </summary>
    public class GamePadButtonInputBinding : InputBinding
    {
        Buttons _physicalButtons;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <param name="name">Name that you use to query the state once it has been registered at the InputBindingAccess.</param>
        /// <param name="buttons">Buttons you want to bind.</param>
        /// <remarks>
        /// This class does not handle thumbstick buttons, use GamePadThumbStickInputBinding for this instead.
        /// </remarks>
        public GamePadButtonInputBinding(PlayerIndex playerIndex, string name, Buttons buttons)
            : base(playerIndex, name)
        {
            _physicalButtons = buttons;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool connected = GamePad.GetState(PlayerIndex).IsConnected;
            GamePadButtons buttons = GamePad.GetState(PlayerIndex).Buttons;
            GamePadDPad dpad = GamePad.GetState(PlayerIndex).DPad;

            bool pressed = true;

            // Buttons
            if (pressed && (Buttons.A & _physicalButtons) != 0)
                pressed = buttons.A == ButtonState.Pressed;

            if (pressed && (Buttons.B & _physicalButtons) != 0)
                pressed = buttons.B == ButtonState.Pressed;

            if (pressed && (Buttons.Back & _physicalButtons) != 0)
                pressed = buttons.Back == ButtonState.Pressed;

            if (pressed && (Buttons.Start & _physicalButtons) != 0)
                pressed = buttons.Start == ButtonState.Pressed;

            if (pressed && (Buttons.X & _physicalButtons) != 0)
                pressed = buttons.X == ButtonState.Pressed;

            if (pressed && (Buttons.Y & _physicalButtons) != 0)
                pressed = buttons.Y == ButtonState.Pressed;

            // DPad
            if (pressed && (Buttons.DPadDown & _physicalButtons) != 0)
                pressed = dpad.Down == ButtonState.Pressed;

            if (pressed && (Buttons.DPadLeft & _physicalButtons) != 0)
                pressed = dpad.Left == ButtonState.Pressed;

            if (pressed && (Buttons.DPadRight & _physicalButtons) != 0)
                pressed = dpad.Right == ButtonState.Pressed;

            if (pressed && (Buttons.DPadUp & _physicalButtons) != 0)
                pressed = dpad.Up == ButtonState.Pressed;

            UpdateState(gameTime, GamePad.GetState(PlayerIndex).IsConnected, pressed ? 1 : 0);
        }

        public override InputBindingState GetState(bool invalidate)
        {
            InputBindingState state = _state;

            if (invalidate)
                _state = InputBindingState.Empty;

            return state;
        }
    }


    /// <summary>
    /// A class that can binds gamepad thumbsticks to a name.
    /// </summary>
    public class GamePadThumbStickInputBinding : InputBinding
    {
        Buttons _physicalButtons;

        /// <summary>
        /// 
        /// </summary>
        static public float DeadZone = 0.1f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <param name="name">Name that you use to query the state once it has been registered at the InputBindingAccess.</param>
        /// <param name="buttons">Thumbstick-Buttons you want to bind.</param>
        public GamePadThumbStickInputBinding(PlayerIndex playerIndex, string name, Buttons buttons)
            : base(playerIndex, name)
        {
            _physicalButtons = buttons;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float value = 0;
            GamePadThumbSticks sticks = GamePad.GetState(PlayerIndex).ThumbSticks;

            // left stick
            if (Math.Abs(value) < float.Epsilon && (Buttons.LeftThumbstickUp & _physicalButtons) != 0)
            {
                if (sticks.Left.Y > DeadZone)
                    value -= sticks.Left.Y;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.LeftThumbstickLeft & _physicalButtons) != 0)
            {
                if (sticks.Left.X < -DeadZone)
                    value += sticks.Left.X;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.LeftThumbstickDown & _physicalButtons) != 0)
            {
                if (sticks.Left.Y < -DeadZone)
                    value -= sticks.Left.Y;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.LeftThumbstickRight & _physicalButtons) != 0)
            {
                if (sticks.Left.X > DeadZone)
                    value += sticks.Left.X;
            }

            // right stick
            if (Math.Abs(value) < float.Epsilon && (Buttons.RightThumbstickUp & _physicalButtons) != 0)
            {
                if (sticks.Right.Y > DeadZone)
                    value -= sticks.Right.Y;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.RightThumbstickLeft & _physicalButtons) != 0)
            {
                if (sticks.Right.X < -DeadZone)
                    value += sticks.Right.X;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.RightThumbstickDown & _physicalButtons) != 0)
            {
                if (sticks.Right.Y < -DeadZone)
                    value -= sticks.Right.Y;
            }

            if (Math.Abs(value) < float.Epsilon && (Buttons.RightThumbstickRight & _physicalButtons) != 0)
            {
                if (sticks.Right.X > DeadZone)
                    value += sticks.Right.X;
            }

            UpdateState(gameTime, GamePad.GetState(PlayerIndex).IsConnected, Math.Abs(value));
        }

        public override InputBindingState GetState(bool invalidate)
        {
            InputBindingState state = _state;

            if (invalidate)
                _state = InputBindingState.Empty;

            return state;
        }
    }

    /// <summary>
    /// Represents a class that contains InputBinding instances and serves
    /// as interface to access them. It provides a method to Add InputBinding's
    /// as well as querying their state. You can add different bindings with the same name.
    /// </remarks>
    public class InputBindingAccess
    {
        List<InputBinding> _bindings = new List<InputBinding>();

        /// <summary>
        /// Must be called every game tick to update the state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            foreach (InputBinding binding in _bindings)
            {
                binding.Update(gameTime);
            }
        }

        /// <summary>
        /// Adds an InputBinding, once added it will be considered in the next Update call.
        /// </summary>
        /// <param name="binding">Binding to add.</param>
        public void Add(InputBinding binding)
        {
            _bindings.Add(binding);
        }

        /// <summary>
        /// Removes all bindings.
        /// </summary>
        public void Clear()
        {
            _bindings.Clear();
        }

        /// <summary>
        /// Returns true when a binding with the specified name has been registered, otherwise false.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public bool Contains(string name)
        {
            foreach (InputBinding binding in _bindings)
            {
                if (string.Compare(binding.Name, name, true) == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the state of InputBinding specified by name.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public InputBindingState GetState(string name)
        {
            return GetState(name, false);
        }

        /// <summary>
        /// Gets the state of InputBinding specified by name and can invalidate it afterwards.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        /// <param name="invalidate">true to invalidate/clear the state</param>
        public InputBindingState GetState(string name, bool invalidate)
        {
            InputBindingState state = InputBindingState.Empty;
            bool found = false;

            foreach (InputBinding binding in _bindings)
            {
                if (string.Compare(binding.Name, name, true) == 0)
                {
                    InputBindingState current = binding.GetState(invalidate);
                    if (!found)
                    {
                        state = current;
                        found = true;
                    }
                    else if (current.Value > float.Epsilon || current.Value < -float.Epsilon && current.PressedMilliseconds > state.PressedMilliseconds)
                        state = current;
                }
            }

            return state;
        }

        /// <summary>
        /// Gets the pressed state of InputBinding specified by name.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public bool GetPressed(string name)
        {
            return GetPressed(name, false);
        }

        /// <summary>
        /// Gets the pressed state of InputBinding specified by name and can invalidate it afterwards.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        /// <param name="invalidate">true to invalidate/clear the state</param>
        public bool GetPressed(string name, bool invalidate)
        {
            return GetState(name, invalidate).Pressed;
        }

        /// <summary>
        /// Gets how many milliseconds elapsed since the InputBinding has been pressed.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public float GetPressedMilliseconds(string name)
        {
            return GetPressedMilliseconds(name, false);
        }


        public float GetPressedMilliseconds(string name, bool invalidate)
        {
            float milliseconds = 0;

            // Find binding with most elapsed milliseconds
            foreach (InputBinding binding in _bindings)
            {
                if (string.Compare(binding.Name, name, true) == 0)
                {
                    InputBindingState state = binding.GetState(invalidate);
                    milliseconds = Math.Max(milliseconds, state.PressedMilliseconds);
                }
            }

            return milliseconds;
        }

        /// <summary>
        /// Gets the released state of InputBinding specified by name.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public bool GetReleased(string name)
        {
            return GetReleased(name, false);
        }

        public bool GetReleased(string name, bool invalidate)
        {
            return GetState(name, invalidate).Released;
        }

        /// <summary>
        /// Gets how many milliseconds elapsed since the InputBinding has been released.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public float GetReleasedMilliseconds(string name)
        {
            return GetReleasedMilliseconds(name, false);
        }

        public float GetReleasedMilliseconds(string name, bool invalidate)
        {
            float milliseconds = float.PositiveInfinity;

            // Find binding with less elapsed milliseconds
            foreach (InputBinding binding in _bindings)
            {
                if (string.Compare(binding.Name, name, true) == 0)
                {
                    InputBindingState state = binding.GetState(invalidate);
                    milliseconds = Math.Min(milliseconds, state.ReleasedMilliseconds);
                }
            }

            return milliseconds;
        }

        /// <summary>
        /// Gets the clicked state of InputBinding specified by name.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public bool GetClicked(string name)
        {
            return GetClicked(name, false);
        }

        public bool GetClicked(string name, bool invalidate)
        {
            return GetState(name, invalidate).Clicked;
        }

        /// <summary>
        /// Gets how many milliseconds elapsed since InputBinding has been clicked.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public float GetClickedMilliseconds(string name)
        {
            return GetClickedMilliseconds(name, false);
        }

        public float GetClickedMilliseconds(string name, bool invalidate)
        {
            float milliseconds = float.PositiveInfinity;

            // Find binding with less elapsed milliseconds
            foreach (InputBinding binding in _bindings)
            {
                if (string.Compare(binding.Name, name, true) == 0)
                {
                    InputBindingState state = binding.GetState(invalidate);
                    milliseconds = Math.Min(milliseconds, state.ClickedMilliseconds);
                }
            }

            return milliseconds;
        }

        /// <summary>
        /// Gets the value of InputBinding specified by name.
        /// The value depends on the InputBinding type, usually it's 1 when pressed and 0 when released.
        /// </summary>
        /// <param name="name">InputBinding name</param>
        public float GetValue(string name)
        {
            return GetValue(name, false);
        }

        public float GetValue(string name, bool invalidate)
        {
            return GetState(name, invalidate).Value;
        }
    }
}
