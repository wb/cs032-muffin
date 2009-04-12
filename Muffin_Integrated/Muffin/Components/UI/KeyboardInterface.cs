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

namespace Muffin.Components.UI
{
    class KeyboardInterface
    {
        float sensitivity;
        int timeBeforeRepeat;
        int timeBeforeInitialRepeat;
        GameObject _gameObject;
        ButtonManager space;

        public KeyboardInterface(GameObject gameObject)
        {
            _gameObject = gameObject;

            // set some default values
            sensitivity = 0.50f;
            timeBeforeInitialRepeat = 100;
            timeBeforeRepeat = 5000;

            space = new ButtonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);

        }

        public void Update(GameTime gameTime)
        {
            // get the state of the controller
            KeyboardState k = Keyboard.GetState();

            int leftRightState = 0, upDownState = 0;

            if (k.IsKeyDown(Keys.A))
                leftRightState -= 1;
            if (k.IsKeyDown(Keys.D))
                leftRightState += 1;

            if (k.IsKeyDown(Keys.W))
                upDownState += 1;
            if (k.IsKeyDown(Keys.S))
                upDownState -= 1;


            // update the buttons
            space.update((k.IsKeyDown(Keys.Space) ? 1 : 0), gameTime.TotalGameTime.TotalMilliseconds);

            float velocity = 3.5f;

            _gameObject.controlInput(new Vector2(-velocity * leftRightState, velocity * upDownState), (space.getButtonState() == 1));

        }
    }
}
