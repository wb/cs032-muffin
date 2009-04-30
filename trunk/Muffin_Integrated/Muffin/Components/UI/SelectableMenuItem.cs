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
    public class SelectableMenuItem : MenuItem
    {
        private Texture2D _textureSelected;
        private Boolean _selected;
        private menuCallback _callback;
        private Vector2 _shadowOffset;


        public SelectableMenuItem(String name, Rectangle rectangle, MuffinGame game, menuCallback callback)
            : base(name, rectangle, game)
        {
            try
            {
                _textureSelected = game.Content.Load<Texture2D>("Textures\\" + name + "Selected");
            }
            catch
            {
                Console.WriteLine("The menu texture: " + name + "Selected was not found.");
                _error = true;
            }
            _callback = callback;
            float xRatio = game.graphics.PreferredBackBufferHeight / 1920.0f;
            float yRatio = game.graphics.PreferredBackBufferWidth / 1200.0f;
            _shadowOffset = new Vector2((int)Math.Round(xRatio * 12.5), (int)Math.Round(yRatio * 6.5));



        }

        public void setSelected(Boolean selected)
        {
            _selected = selected;
        }

        /*
         * This returns the texture, normal if not selected, or selected otherwise.
         * */

        public override Texture2D currentTexture()
        {
            if (!_selected)
                return _texture;
            else
                return _textureSelected;
        }

        public override Rectangle currentRectangle()
        {
            if (!_selected)
                return base.currentRectangle();

            return new Rectangle(_rectangle.X + (int)_shadowOffset.X, _rectangle.Y - (int)_shadowOffset.Y, _rectangle.Width, _rectangle.Height);
        }
        /*
         * This method executes the callback, only if this button is selected
         * and there is a non null callback.
         * */

        public void executeCallback()
        {
            if (_selected && _callback != null)
            {
                _callback();
            }
        }
    }
}
