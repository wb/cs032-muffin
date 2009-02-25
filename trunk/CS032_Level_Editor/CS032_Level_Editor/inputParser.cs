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

namespace CS032_Level_Editor
{
    class inputParser
    {
        Game1 game;

        float sensitivity;
        int timeBeforeRepeat;
        int timeBeforeInitialRepeat;

        buttonManager thumbStickLeftX, thumbStickLeftY, leftRightArrowKeys, upDownArrowKeys, selectValue, unselectValue;
        
        public inputParser(Game1 g)
        {

            game = g;

            sensitivity = 0.50f;
            timeBeforeInitialRepeat = 400;
            timeBeforeRepeat = 100;

            
            thumbStickLeftX = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            thumbStickLeftY = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);

            leftRightArrowKeys = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            upDownArrowKeys = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);

            selectValue = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);
            unselectValue = new buttonManager(sensitivity, timeBeforeRepeat, timeBeforeInitialRepeat);

        }


        public void update(double gameTime)
        {
            GamePadState g = GamePad.GetState(PlayerIndex.One);
            KeyboardState k = Keyboard.GetState();

            thumbStickLeftX.update(g.ThumbSticks.Left.X, gameTime);
            thumbStickLeftY.update(g.ThumbSticks.Left.Y, gameTime);

            int leftRightValue = 0;
            if(k.IsKeyDown(Keys.Left))
                leftRightValue -= 1;
            if(k.IsKeyDown(Keys.Right))
                leftRightValue += 1;
            leftRightArrowKeys.update(leftRightValue, gameTime);

            int upDownValue = 0;
            if(k.IsKeyDown(Keys.Up))
                upDownValue += 1;
            if (k.IsKeyDown(Keys.Down))
                upDownValue -= 1;
            upDownArrowKeys.update(upDownValue, gameTime);

        }

        

        public Boolean left()
        {
            return (this.leftThumbStickValueX(-1) || this.leftRightArrowKeyValues(-1));
        }

        public Boolean right()
        {
            return (this.leftThumbStickValueX(1) || this.leftRightArrowKeyValues(1));
        }

        public Boolean up()
        {
            return (this.leftThumbStickValueY(1) || this.upDownArrowKeyValues(1));
        }

        public Boolean down()
        {
            return (this.leftThumbStickValueY(-1) || this.upDownArrowKeyValues(-1));
        }

        public Boolean select()
        {
            return false;
        }

        public Boolean unselect()
        {
            return false;
        }





        private Boolean leftThumbStickValueX(int value)
        {
            return (value == thumbStickLeftX.getButtonState());

        }

        private Boolean leftThumbStickValueY(int value)
        {
            return (value == thumbStickLeftY.getButtonState());
        }

        private Boolean leftRightArrowKeyValues(int value)
        {
            return (value == leftRightArrowKeys.getButtonState());
        }

        private Boolean upDownArrowKeyValues(int value)
        {
            return (value == upDownArrowKeys.getButtonState());
        }

    }
}
