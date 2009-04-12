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
            buttonA = new ButtonManager(sensitivity, 5000, 5000);

        }

        public void Update(GameTime gameTime)
        {
            // get the state of the controller
            GamePadState g = GamePad.GetState(_playerIndex);

            // update the left thumbsticks
            thumbStickLeftX.update(g.ThumbSticks.Left.X, gameTime.TotalGameTime.TotalMilliseconds);
            thumbStickLeftY.update(g.ThumbSticks.Left.Y, gameTime.TotalGameTime.TotalMilliseconds);
            buttonA.update((g.Buttons.A == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);

            //_gameObject.position = _gameObject.position + new Vector3(-10.0f * g.ThumbSticks.Left.X, 0, 0);
            //_gameObject.position = _gameObject.position + new Vector3(0, 0, 10.0f * g.ThumbSticks.Left.Y);
            float velocity = 3.5f;

            if(GamePad.GetState(_playerIndex).IsConnected)
                _gameObject.controlInput(new Vector2(-velocity * g.ThumbSticks.Left.X, velocity * g.ThumbSticks.Left.Y), (buttonA.getButtonState() == 1));

        }


    }
}