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
using Muffin;
using Definitions;
using Muffin.Components.Renderer;

namespace Muffin.Components.UI
{
    class UiObject
    {
        private Texture2D _texture;
        private VertexPositionTexture[] _vertices;

        public UiObject(Texture2D tex, Rectangle rect)
        {
            _texture = tex;
            _vertices = new VertexPositionTexture[4];
            
            _vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0.0f), new Vector2(0, 0));
            _vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0.0f), new Vector2(1, 0));
            _vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0.0f), new Vector2(0, 1));
            _vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0.0f), new Vector2(1, 1));
            
        }

        public Texture2D texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        public VertexPositionTexture[] vertices
        {
            get { return _vertices; }
            set { _vertices = value; } 
        }
    }
}
