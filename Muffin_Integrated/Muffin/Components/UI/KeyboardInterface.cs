using System;
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
        int timeBeforeRepeat, timeBeforeInitialRepeat, _previousX, _previousY;
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
                leftRightState -= 1;
            if (k.IsKeyDown(Keys.D))
                leftRightState += 1;

            if (k.IsKeyDown(Keys.W))
                upDownState += 1;
            if (k.IsKeyDown(Keys.S))
                upDownState -= 1;

            if (k.IsKeyDown(Keys.Q))
                strafeState += 1;
            if (k.IsKeyDown(Keys.E))
                strafeState -= 1;

            // mouse stuff (will override input from keyboard, for stuff like rotating the character)

            int deltaX = m.X - _previousX;
            _previousX = m.X;

            int deltaY = m.Y - _previousY;
            _previousY = m.Y;

            if (m.LeftButton == ButtonState.Pressed)
            {
                camera.updateLookRotation((float)deltaX / -500.0f, (float)deltaY / 500.0f);
            }

            if (m.RightButton == ButtonState.Pressed)
            {

                leftRightState = (float)deltaX / 5.0f;

            }

            // update the buttons
            space.update((k.IsKeyDown(Keys.Space) ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);


            float yaw = leftRightState * MathHelper.ToRadians(-1.0f);
            Quaternion rot = Quaternion.CreateFromAxisAngle(Vector3.Up, yaw);
            _gameObject.orientation *= rot;
            Vector3 direction = Vector3.Transform(new Vector3(0.0f, 0.0f, upDownState * 2.0f), Matrix.CreateFromQuaternion(_gameObject.orientation));

            Vector3 strafe;
            // strafing
            if (strafeState != 0)
            {
                strafe = Vector3.UnitZ;
                float strafeAngle = (float)(Math.PI) / 2.0f * strafeState;
                strafe = 2.0f * Vector3.Transform(strafe, Matrix.CreateFromQuaternion(_gameObject.orientation) * Matrix.CreateFromAxisAngle(Vector3.Up, strafeAngle));
            }
            else
            {
                strafe = Vector3.Zero;
            }

            //direction = direction + strafe;
            _gameObject.controlInput(new Vector2(direction.X + strafe.X, direction.Z + strafe.Z), (space.getButtonState() == 1));



        }
    }
}
