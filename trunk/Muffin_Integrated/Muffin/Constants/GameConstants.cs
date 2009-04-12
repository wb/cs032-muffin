using System;
using System.Collections.Generic;
using System.Text;
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

namespace Definitions
{
    public class GameConstants
    {
        public const string CurrentLevel = "level1";
        public const float NearClip = 100.0f;
        public const float FarClip = 1500.0f;
        public const float ViewAngle = 45.0f;
        public const int MaxLights = 3;
        public const int MaxAcceleration = 2000;
    }
}
