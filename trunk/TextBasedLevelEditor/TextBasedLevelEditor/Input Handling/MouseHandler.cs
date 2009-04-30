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
    class MouseHandler
    {
        private int _gridStartX, _gridStartY;
        private Vector2 _position;
        private Game1 _game;
        private MouseState _currentState;
        private MouseState _previousState;

        public MouseHandler(int gridStartX, int gridStartY, Game1 game)
        {
            _gridStartX = gridStartX;
            _gridStartY = gridStartY;
            _game = game;

            // mouse stuff
            _currentState = Mouse.GetState();
            _previousState = _currentState;
        }

        public Vector2 getGridPosition()
        {
            _currentState = Mouse.GetState();

            int x = _currentState.X;
            int y = _currentState.Y;

            int currentX = (x - _gridStartX) / Constants.gridSize;
            int currentY = (y - _gridStartY) / Constants.gridSize;

            // if the mouse is on the grid, update the position
            if (currentX >= 0 && currentX < Constants.gridSizeX && currentY >= 0 && currentY < Constants.gridSizeY)
            {
                _position.X = currentX;
                _position.Y = currentY;

                // if they click, add this object!
                if (_currentState.LeftButton == ButtonState.Pressed)// && _previousState.LeftButton != ButtonState.Pressed)
                {
                    _game.addSelectedObjectAtPosition(_position);
                }

                // if they right click, delete this object
                if (_currentState.RightButton == ButtonState.Pressed)
                {
                    _game.removeObjectAtPosition(_position);
                }

                // if they middle click, only delete objects of the selected type
                if (_currentState.MiddleButton == ButtonState.Pressed)
                {
                    _game.removeObjectOfTypeAtPosition(_position);
                }

            }

            _previousState = _currentState;

            return _position;
        }
    }
}
