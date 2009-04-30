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
    class SelectedObject
    {
        private Texture2D _sprite;
        private Rectangle _rectangle;
        private String _modelName;
        private Game _game;
        private MenuCallback _callback;

        public SelectedObject(String modelName, Game game, Vector3 position, MenuCallback callback)
        {
            _sprite = game.Content.Load<Texture2D>("Tiles\\" + modelName);
            _rectangle = new Rectangle((int) position.X, (int) position.Z, Constants.gridSize, Constants.gridSize);
            _game = game;
            _modelName = modelName;
            _callback = callback;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _rectangle, Color.White);
        }

        public void updatePosition(Vector2 position)
        {
            _rectangle.X = (int) position.X;
            _rectangle.Y = (int) position.Y;
        }

        public String modelName
        {
            get { return _modelName; }
        }
        public Vector3 position
        {
            get { return new Vector3(_rectangle.X, 0, _rectangle.Y); }
        }

        public void updateSprite(String modelName) {
            _modelName = modelName;
            _sprite = _game.Content.Load<Texture2D>("Tiles\\" + modelName);
        }

        public void callback()
        {
            if(_callback != null)
                _callback();
        }
    }
}
