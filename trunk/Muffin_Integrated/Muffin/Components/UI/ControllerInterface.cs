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
        MuffinGame _muffinGame;


        ButtonManager thumbStickLeftX, thumbStickLeftY, buttonA, buttonY, buttonStart;

        public ControllerInterface(GameObject gameObject, PlayerIndex playerIndex, MuffinGame game)
        {
            _gameObject = gameObject;
            _playerIndex = playerIndex;
            _muffinGame = game;

            // set some default values
            sensitivity = 0.50f;
            timeBeforeInitialRepeat = 400;
            timeBeforeRepeat = 100;

            // instantiate our buttons
            thumbStickLeftX = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            thumbStickLeftY = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            buttonA = new ButtonManager(sensitivity, 100, 400);
            buttonY = new ButtonManager(sensitivity, int.MaxValue, int.MaxValue); // this button can never repeat while held down

            buttonStart = new ButtonManager(sensitivity, int.MaxValue, int.MaxValue); // this button also never repeats
        }

        public Boolean isConnected()
        {
            return GamePad.GetState(_playerIndex).IsConnected;
        }

        public void setPlayerToControl(GameObject player)
        {
            _gameObject = player;
        }

        public void Update(GameTime gameTime, GameCamera camera)
        {
            // get the state of the controller
            GamePadState g = GamePad.GetState(_playerIndex);

            // update the look angle (for looking around)
            camera.updateLookRotation(g.ThumbSticks.Right.X / -50.0f, g.ThumbSticks.Right.Y / -50.0f);
            // and the zoom level
            camera.zoom(15.0f * (g.Triggers.Right - g.Triggers.Left));
            // and the look mode
            camera.lookMode((buttonY.getButtonState() == 1) ? true : false);

            // update the left thumbsticks
            thumbStickLeftX.update(g.ThumbSticks.Left.X, gameTime.TotalGameTime.TotalMilliseconds);
            thumbStickLeftY.update(g.ThumbSticks.Left.Y, gameTime.TotalGameTime.TotalMilliseconds);
            buttonA.update((g.Buttons.A == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);
            buttonY.update((g.Buttons.Y == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);
            buttonStart.update((g.Buttons.Start == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);

            // update the object if we aren't paused
            if (!_muffinGame.paused)
                _gameObject.move(g.ThumbSticks.Left.Y, g.ThumbSticks.Left.X, g.ThumbSticks.Left.X, (g.Buttons.A == ButtonState.Pressed), (g.Buttons.X == ButtonState.Pressed));

            
            // test load next level
            if (buttonY.getButtonState() == 1)
                _muffinGame.levelCompleted();

            // pause if we must pause
            if (buttonStart.getButtonState() == 1)
                _muffinGame.paused = !_muffinGame.paused;
            
        }
    }
}