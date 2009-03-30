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

namespace _3D_Renderer
{
    public enum ModelName { FLAT, WEDGE, CORNER, INVERTED_CORNER, FORKLIFT, BOX, NONE };
    public enum ModelType { TERRAIN, ENEMY, OBJECT, HUMAN };

    class GameConstants
    {
        public const float NearClip = 1.0f;
        public const float FarClip = 2000.0f;
        public const float ViewAngle = 45.0f;
        public const int MaxLights = 3;
    }
}
