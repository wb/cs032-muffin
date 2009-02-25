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
using WindowsGame1;

namespace WindowsGame1
{
    public enum ModelName { FLAT, WEDGE, CORNER, INVERTED_CORNER, NONE };
    public enum ModelType { TERRAIN, ENEMY, OBJECT, HUMAN };

    class GameConstants
    {
        public const float NearClip = 1.0f;
        public const float FarClip = 30000.0f;
        public const float ViewAngle = 45.0f;
    }
}
