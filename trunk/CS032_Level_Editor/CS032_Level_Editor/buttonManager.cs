using System;
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

namespace CS032_Level_Editor
{
    class buttonManager
    {
        int buttonState;
        double timeSinceInitialPress;
        double timeSinceLastPress;

        Boolean buttonIsPressed;

        float sensitivity;
        int timeBeforeRepeat;
        int timeBeforeInitialRepeat;

        public buttonManager(float s, int tbr, int tbir)
        {
            buttonState = 0;
            timeSinceInitialPress = 0;
            timeSinceLastPress = 0;

            buttonIsPressed = false;

            sensitivity = s;
            timeBeforeRepeat = tbr;
            timeBeforeInitialRepeat = tbir;

        }

        public void update(float buttonValue, double gameTime)
        {

            // if they are pressing the button and it isn't currently pressed, set it active
            if (Math.Abs(buttonValue) >= sensitivity && !buttonIsPressed)
            {
                buttonIsPressed = true;
                timeSinceLastPress = gameTime;
                timeSinceInitialPress = gameTime;

                // set the value of the button
                if (buttonValue > 0)
                    buttonState = 1;
                else
                    buttonState = -1;
            }
            else if (Math.Abs(buttonValue) >= sensitivity && buttonIsPressed)
            {
                // calculate time passed since initial press, bitch!
                double timeSinceFirstPress = gameTime - timeSinceInitialPress;

                // if this is the first press, we need a longer delay
                if (timeSinceFirstPress < timeBeforeInitialRepeat)
                    buttonState = 0;
                else
                {
                    // calculate the time passed since last press
                    double timePassed = gameTime - timeSinceLastPress;

                    // if enough time has passed, set value
                    if (timePassed > timeBeforeRepeat)
                    {
                        buttonIsPressed = true;
                        timeSinceLastPress = gameTime;

                        // set the value
                        if (buttonValue > 0)
                            buttonState = 1;
                        else
                            buttonState = -1;
                    }
                    else
                        buttonState = 0;
                }
            }
            else
            {
                buttonIsPressed = false;
                timeSinceInitialPress = 0;
                timeSinceLastPress = 0;
                buttonState = 0;
            }
        }

        public int getButtonState()
        {
            return buttonState;
        }
    }
}
