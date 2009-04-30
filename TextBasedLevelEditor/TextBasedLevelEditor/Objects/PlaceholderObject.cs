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
    class PlaceholderObject : DrawableObject
    {
        private LevelObject _levelObject;

        public PlaceholderObject(LevelObject levelObject) : base(levelObject.modelName + "Placeholder", levelObject.game, levelObject.position)
        {
            _levelObject = levelObject;
        }

        public LevelObject getInternalObject()
        {
            return _levelObject;
        }
    }
}
