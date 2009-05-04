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


        ButtonManager thumbStickLeftX, thumbStickLeftY, buttonA, buttonY, buttonStart, thumbStickRightY;

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
            thumbStickRightY = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            buttonA = new ButtonManager(sensitivity, int.MaxValue, int.MaxValue);
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
            thumbStickRightY.update(g.ThumbSticks.Right.Y, gameTime.TotalGameTime.TotalMilliseconds);
            buttonA.update((g.Buttons.A == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);
            buttonY.update((g.Buttons.Y == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);
            buttonStart.update((g.Buttons.Start == ButtonState.Pressed ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);



            // update the object if we aren't paused
            if (!_muffinGame.paused)
            {
                // if we are jumping, play sounds
                if (_gameObject.jumpCount < 2 && (buttonA.getButtonState() == 1))
                    _muffinGame.playSoundClip("jump");

                // controls
                float leftRight = g.ThumbSticks.Left.X;
                float upDown = g.ThumbSticks.Left.Y;
                float strafeLeftRight = g.ThumbSticks.Left.X;

                // add d-pad controls for strafing to make life easier
                Boolean strafing = ((g.Buttons.X == ButtonState.Pressed) || g.DPad.Down == ButtonState.Pressed || g.DPad.Up == ButtonState.Pressed || g.DPad.Left == ButtonState.Pressed || g.DPad.Right == ButtonState.Pressed);

                if (g.DPad.Left == ButtonState.Pressed || g.DPad.Right == ButtonState.Pressed)
                    strafeLeftRight += (g.DPad.Left == ButtonState.Pressed ? -1 : 0) + (g.DPad.Right == ButtonState.Pressed ? 1 : 0);
                if(g.DPad.Up == ButtonState.Pressed || g.DPad.Down == ButtonState.Pressed)
                    upDown += (g.DPad.Up == ButtonState.Pressed ? 1 : 0) + (g.DPad.Down == ButtonState.Pressed ? -1 : 0);

                _gameObject.move(upDown, leftRight, strafeLeftRight, (buttonA.getButtonState() == 1 ? true : false), strafing);
            }


            // test load next level
            if (buttonY.getButtonState() == 1)
                _muffinGame.starCollected();

            // pause if we must pause
            if (buttonStart.getButtonState() == 1)
                _muffinGame.togglePauseMenu();

            // input for menus
            _muffinGame.menuInput(-thumbStickLeftY.getButtonState(), buttonA.getButtonState() == 1 ? true : false);

        }
    }
}