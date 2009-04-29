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
using Muffin.Objects;


namespace Muffin.Components.UI
{
    public class MenuItem
    {
        protected Texture2D _texture;
        protected Rectangle _rectangle;
        protected Boolean _error;

        public MenuItem(String name, Rectangle rectangle, MuffinGame game)
        {
            try
            {
                _texture = game.Content.Load<Texture2D>("Textures\\" + name);
                _error = false;
            }
            catch
            {
                _error = true;
                Console.WriteLine("Error loading menu texture: " + name);
            }
            _rectangle = rectangle;
        }

      
        /*
         * This returns the texture.
         * */

        public virtual Texture2D currentTexture()
        {
            return _texture;
        }

        public virtual Rectangle currentRectangle()
        {
            return _rectangle;
        }

        public virtual void setRectangle(Rectangle rect)
        {
            _rectangle = rect;
        }

        public Boolean error
        {
            get { return _error; }
        }
    }
}
