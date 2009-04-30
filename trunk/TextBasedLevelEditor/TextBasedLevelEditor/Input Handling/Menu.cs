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
    public delegate void MenuCallback();

    class Menu
    {
        private int _gridStartX, _gridStartY, _gridWidth, _gridHeight;
        private Vector2 _position;
        private Game1 _game;
        private MouseState _currentState;
        private MouseState _previousState;
        private SelectedObject[,] _menuItems;
        private SelectedObject _lastSelectedObject;
        

        public Menu(int gridStartX, int gridStartY, Game1 game)
        {
            _gridStartX = gridStartX;
            _gridStartY = gridStartY;
            _gridWidth = 2;
            _gridHeight = 3;

            _game = game;

            // construct the menu grid
            _menuItems = new SelectedObject[_gridWidth, _gridHeight];
            _menuItems[0, 0] = new SelectedObject("grass", game, this.position(0, 0), new MenuCallback(updateSelectedCallback));
            _menuItems[1, 0] = new SelectedObject("box", game, this.position(1, 0), new MenuCallback(updateSelectedCallback));
            _menuItems[0, 1] = new SelectedObject("player", game, this.position(0, 1), new MenuCallback(updateSelectedCallback));
            _menuItems[1, 1] = new SelectedObject("ai", game, this.position(1, 1), new MenuCallback(updateSelectedCallback));
            _menuItems[0, 2] = new SelectedObject("coin", game, this.position(0, 2), new MenuCallback(updateSelectedCallback));
            _menuItems[1, 2] = new SelectedObject("star", game, this.position(1, 2), new MenuCallback(updateSelectedCallback));
            

            // mouse stuff
            _currentState = Mouse.GetState();
            _previousState = _currentState;
        }

        public void updateSelected()
        {
            _currentState = Mouse.GetState();

            int x = _currentState.X;
            int y = _currentState.Y;

            int currentX = (x - _gridStartX) / Constants.gridSize;
            int currentY = (y - _gridStartY) / Constants.gridSize;

            // if the mouse is on the grid, update the position
            if (currentX >= 0 && currentX < _gridWidth && currentY >= 0 && currentY < _gridHeight)
            {
                _position.X = currentX;
                _position.Y = currentY;

                // if they click, see if there is a tile here, and select it if there is
                if (_currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton != ButtonState.Pressed)
                {
                    if (_menuItems[currentX, currentY] != null)
                    {
                        _lastSelectedObject = _menuItems[currentX, currentY];
                        _menuItems[currentX, currentY].callback();
                    }
                        
                }

            }

            _previousState = _currentState;

            
        }

        // helper function for getting position
        private Vector3 position(int x, int y)
        {
            int xPosition = _gridStartX + x * Constants.gridSize;
            int zPosition = _gridStartY + y * Constants.gridSize;

            return new Vector3(xPosition, 0, zPosition);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_menuItems[x, y] != null)
                        _menuItems[x, y].draw(spriteBatch);
                }
            }
        }

        private void updateSelectedCallback()
        {
            _game.updateSelectedObject(_lastSelectedObject.modelName);
        }
    }
}
