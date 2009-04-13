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
using Muffin;
using Definitions;
using Muffin.Components.Renderer;

namespace Muffin.Components.UI
{
    public class ControllerInterface
    {
        float sensitivity;
        int timeBeforeRepeat;
        int timeBeforeInitialRepeat;
        PlayerIndex _playerIndex;
        GameObject _gameObject;


        ButtonManager thumbStickLeftX, thumbStickLeftY, buttonA;

        public ControllerInterface(GameObject gameObject, PlayerIndex playerIndex)
        {
            _gameObject = gameObject;
            _playerIndex = playerIndex;

            // set some default values
            sensitivity = 0.50f;
            timeBeforeInitialRepeat = 400;
            timeBeforeRepeat = 100;

            // instantiate our buttons
            thumbStickLeftX = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            thumbStickLeftY = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            buttonA = new ButtonManager(sensitivity, 100, 400);

        }

        public Boolean isConnected()
        {
            return GamePad.GetState(_playerIndex).IsConnected;
        }

        public void Update(GameTime gameTime, GameCamera camera)
        {
            // get the state of the controller
            GamePadState g = GamePad.GetState(_playerIndex);

            // update the look angle (for looking around)

            camera.updateLookRotation(g.ThumbSticks.Right.X / -50.0f, g.ThumbSticks.Right.Y / -50.0f);

            // update the left thumbsticks
            thumbStickLeftX.update(g.ThumbSticks.Left.X, gameTime.TotalGameTime.TotalMilliseconds);
            thumbStickLeftY.update(g.ThumbSticks.Left.Y, gameTime.TotalGameTime.TotalMilliseconds);
            buttonA.update((g.Buttons.A == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);

            Vector3 direction;
            Vector3 strafe;

            // if x is pressed, left thumbstick x will be used for strafing
            if (g.Buttons.X == ButtonState.Pressed)
            {
                float strafeState = (g.ThumbSticks.Left.X == 0 ? 0 : 1);
                strafe = Vector3.UnitZ;
                float strafeAngle = (float)(Math.PI) / 2.0f * strafeState;
                strafe = -g.ThumbSticks.Left.X * 2.0f * Vector3.Transform(strafe, Matrix.CreateFromQuaternion(_gameObject.orientation) * Matrix.CreateFromAxisAngle(Vector3.Up, strafeAngle));

                direction = Vector3.Transform(new Vector3(0.0f, 0.0f, g.ThumbSticks.Left.Y * 2.0f), Matrix.CreateFromQuaternion(_gameObject.orientation));
            }
            // otherwise it will be used for movement
            else
            {
                float yaw = g.ThumbSticks.Left.X * MathHelper.ToRadians(-1.7f);
                Quaternion rot = Quaternion.CreateFromAxisAngle(Vector3.Up, yaw);
                _gameObject.orientation *= rot;
                direction = Vector3.Transform(new Vector3(0.0f, 0.0f, g.ThumbSticks.Left.Y * 2.0f), Matrix.CreateFromQuaternion(_gameObject.orientation));

                strafe = Vector3.Zero;
            }


            //direction = direction + strafe;
            _gameObject.controlInput(new Vector2(direction.X + strafe.X, direction.Z + strafe.Z), (buttonA.getButtonState() == 1));

        }
    }
}