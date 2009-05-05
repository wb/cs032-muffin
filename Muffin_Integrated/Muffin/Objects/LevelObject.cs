using System;
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

namespace Muffin.Objects
{
    /*
     * This class needs to be filled out/changed, but in essence, it should hold anything and
     * everything that is needed for the game to know how to play and complete the level.
     * */

    public class LevelObject
    {
        private CollectableObject _goal;
        private float _ceiling, _floor; // these are the max and min height of the level (players cant go above or below this level)
        private XmlDocument _levelFile; // this is the file containing the definition for this level
        private String _levelName;

        public LevelObject(String levelName)
        {
            // load the xml file corresponding to the current level
            if (File.Exists("Content\\Levels\\" + levelName + ".xml"))
            {
                _levelFile = new XmlDocument();
                _levelFile.Load("Content\\Levels\\" + levelName + ".xml");
                _levelName = levelName;
                
            }
            else
            {
                Console.WriteLine("The file " + "Content\\Levels\\" + levelName + ".xml" + " was not found");
            }
            
        }

        public XmlDocument levelFile
        {
            get { return _levelFile; }
            set { _levelFile = value; }
        }

        public float ceiling
        {
            get { return _ceiling; }
            set { _ceiling = value; }
        }

        public float floor
        {
            get { return _floor; }
            set { _floor = value; }
        }

        public CollectableObject goal
        {
            get { return _goal; }
            set { _goal = value; }
        }

        public String levelName
        {
            get { return _levelName; }
        }
    }
}
