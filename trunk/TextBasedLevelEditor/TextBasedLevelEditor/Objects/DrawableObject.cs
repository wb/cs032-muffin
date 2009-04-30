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
    class DrawableObject
    {
        private Texture2D _sprite;
        private Rectangle _rectangle;

        public DrawableObject(String modelName, Game game, Vector3 position)
        {
            _sprite = game.Content.Load<Texture2D>("Tiles\\" + modelName);
            _rectangle = new Rectangle((int) position.X / Constants.objectToGridRatio, (int) position.Z / Constants.objectToGridRatio, Constants.gridSize, Constants.gridSize);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _rectangle, Color.White);
        }
    }
}
