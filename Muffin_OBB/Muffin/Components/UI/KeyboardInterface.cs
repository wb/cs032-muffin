﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Muffin;
using Definitions;
using Muffin.Components.Renderer;

namespace Muffin.Components.UI
{
    class KeyboardInterface
    {
        float sensitivity;
        int timeBeforeRepeat, timeBeforeInitialRepeat, _previousX, _previousY, _previousScroll;
        GameObject _gameObject;
        ButtonManager space;

        public KeyboardInterface(GameObject gameObject)
        {
            _gameObject = gameObject;

            // set some default values
            sensitivity = 0.50f;
            timeBeforeInitialRepeat = 100;
            timeBeforeRepeat = 5000;
            _previousX = Mouse.GetState().X;
            _previousY = Mouse.GetState().Y;
            _previousScroll = Mouse.GetState().ScrollWheelValue;
            space = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);

        }

        public void Update(GameTime gameTime, GameCamera camera)
        {
            // get the state of the controller
            KeyboardState k = Keyboard.GetState();
            MouseState m = Mouse.GetState();
            float leftRightState = 0, upDownState = 0, strafeState = 0;

            // keyboard stuff
            if (k.IsKeyDown(Keys.A))
                strafeState -= 1;
            if (k.IsKeyDown(Keys.D))
                strafeState += 1;

            if (k.IsKeyDown(Keys.W))
                upDownState += 1;
            if (k.IsKeyDown(Keys.S))
                upDownState -= 1;

            if (k.IsKeyDown(Keys.Q))
            {
                leftRightState -= 1;
            }
            if (k.IsKeyDown(Keys.E))
                leftRightState += 1;

            // mouse stuff (will override input from keyboard, for stuff like rotating the character)

            int deltaX = m.X - _previousX;
            _previousX = m.X;

            int deltaY = m.Y - _previousY;
            _previousY = m.Y;

            int deltaScroll = m.ScrollWheelValue - _previousScroll;
            _previousScroll = m.ScrollWheelValue;

            if (m.LeftButton == ButtonState.Pressed)
                camera.updateLookRotation((float)deltaX / -500.0f, (float)deltaY / 500.0f);
            if (m.RightButton == ButtonState.Pressed)
                leftRightState = (float)deltaX / 5.0f;
  
            // control the zoom
            camera.zoom((float)deltaScroll);



            // update the buttons
            space.update((k.IsKeyDown(Keys.Space) ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);

            // input updown state (normalized to 1), left right state (normalized to 1), strafe state (normalized to 1), jump boolean, and strafe boolean
            _gameObject.move(upDownState, leftRightState, strafeState, (space.getButtonState() == 1), (k.IsKeyDown(Keys.A) || k.IsKeyDown(Keys.D)));

           


        }
    }
}
