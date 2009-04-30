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
using System.Xml;
using System.IO;
using System.Text;

namespace TextBasedLevelEditor
{
    class KeyboardHandler
    {
        private KeyboardState _currentState, _previousState;

        public KeyboardHandler()
        {
            _currentState = Keyboard.GetState();
            _previousState = _currentState;
        }

        public void update()
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();
        }
        public Boolean keyPressed(Keys key)
        {
            if (_currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key))
                return true;
            else
                return false;
        }
    }
}
