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
    class LevelObject : DrawableObject
    {
        protected String _modelName;
        protected Vector3 _position, _size;
        protected float _rotation;
        protected Game _game;

        public LevelObject(String modelName, Vector3 position, Vector3 size, float rotation, Game game) : base(modelName, game, position)
        {
            _modelName = modelName;
            _position = position;
            _size = size;
            _rotation = rotation;
            _game = game;
        }

        public String modelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }

        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 size
        {
            get { return _size; }
            set { _size = value; }
        }

        public float rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Game game
        {
            get { return _game; }
        }
    }
}
