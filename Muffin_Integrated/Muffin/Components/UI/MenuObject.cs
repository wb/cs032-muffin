﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Definitions;
using Muffin.Components.Renderer;
using Muffin.Components.Physics;
using Muffin.Components.UI;
using Muffin.Components.AI;
using Muffin.Objects;


namespace Muffin.Components.UI
{
    public class MenuObject
    {
        private List<MenuItem> _items;
        private List<SelectableMenuItem> _selectableItems;
        private SpriteBatch _spriteBatch;
        private Boolean _hidden;
        private MuffinGame _muffinGame;
        private int _currentItemIndex;
        private List<SoundEffect> _soundEffects;

        public MenuObject(SpriteBatch spriteBatch, MuffinGame game, List<SoundEffect> soundEffects)
        {
            _spriteBatch = spriteBatch;
            _items = new List<MenuItem>();
            _selectableItems = new List<SelectableMenuItem>();
            _hidden = true;
            _muffinGame = game;
            _currentItemIndex = 0;
            _soundEffects = soundEffects;
        }

        /*
         * This method takes a rectangle as if it were placed on a 1920 by 1200
         * screen.  It then scalse it down to fit whatever screen size is
         * currently set. Purely for convenience.
         * */

        public void addItem(String name, Rectangle rectangle, Boolean selectable, menuCallback callback)
        {
            double width = _muffinGame.graphics.PreferredBackBufferWidth;
            double height = _muffinGame.graphics.PreferredBackBufferHeight;

            double ratioWidth = width / 1920;
            double ratioHeight = height / 1200;

            Rectangle adjustedRectangle = new Rectangle((int)Math.Round(((double)rectangle.X) * ratioWidth), (int)Math.Round(((double)rectangle.Y) * ratioHeight), (int)Math.Round(((double)rectangle.Width) * ratioWidth), (int)Math.Round(((double)rectangle.Height) * ratioHeight));

            if (selectable)
            {
                SelectableMenuItem menuItem = new SelectableMenuItem(name, adjustedRectangle, _muffinGame, callback);
                _items.Add(menuItem);
                _selectableItems.Add(menuItem);

                // if this is the first item, select it initially
                if (_selectableItems.Count() == 1)
                    _selectableItems.ElementAt(0).setSelected(true);
            }
            else
                _items.Add(new MenuItem(name, adjustedRectangle, _muffinGame));
        }

        public void draw()
        {
            if (!_hidden)
            {
                foreach (MenuItem item in _items)
                {
                    _spriteBatch.Draw(item.currentTexture(), item.currentRectangle(), Color.White);
                }
            }

        }

        public Boolean hidden
        {
            get { return _hidden; }
            set
            {
                // play a sound when the menu is shown or hidden
                if(value != _hidden)
                    _soundEffects.ElementAt((int)SoundClip.SELECT).Play();

                _hidden = value;
                if (_hidden == true)
                {
                    if (_selectableItems.Count() > 0)
                    {
                        // reset the menu so it starts on the first item next time
                        _selectableItems.ElementAt(_currentItemIndex).setSelected(false);
                        _currentItemIndex = 0;
                        _selectableItems.ElementAt(_currentItemIndex).setSelected(true);
                    }
                }
                else
                {
                    
                }
            }
        }

        /*
         * Pass in a 1 to move down, -1 to move up.
         * */

        public void menuInput(int direction, Boolean select)
        {   

            // only move if it isn't hidden and we aren't selecting
            if (!hidden && !select)
            {
                if (direction == 0)
                    return;

                // make sure our input is -1 or 1
                if (direction > 0)
                    direction = 1;
                if (direction < 0)
                    direction = -1;

                // deselect the current item
                _selectableItems.ElementAt(_currentItemIndex).setSelected(false);

                // update current item
                _currentItemIndex = (_currentItemIndex + direction) % _selectableItems.Count();

                // force it to be positive
                if (_currentItemIndex < 0)
                    _currentItemIndex += _selectableItems.Count();

                // select current item
                _selectableItems.ElementAt(_currentItemIndex).setSelected(true);

                // play the sound
                _soundEffects.ElementAt((int)SoundClip.CHANGE).Play();
            }
            // if the user selects something, perform the callback
            else if (!hidden && select)
            {
                _selectableItems.ElementAt(_currentItemIndex).executeCallback();
           
            }
        }

    }
}
