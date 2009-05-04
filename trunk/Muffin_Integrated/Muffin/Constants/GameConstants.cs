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
        public const float ViewAngle = 45.0f;
        public const int MaxLights = 1;
        public const int MaxAcceleration = 2000;
        public const int MaxMoveWeightRatio = 10;
        public const int MaxHeight = 1000; // this is the max height that an object can move (jumping/flying stuff)
        public const int MinHeightPlayer = -400; // this is the floor of the game.  you will die if you hit this
        public const int MinHeightObject = -2000; // make this far enough so you cant see the boxes disappearing 
        public const float GameObjectScale = 10.0f; // this is the universal scale constant for objects
        public const float NearClip = 1000.0f;
        public const float FarClip = 100000.0f;

        public const bool FULL_SCREEN_ENABLED = false;
        public const int SCREEN_WIDTH = 1280;
        public const int SCREEN_HEIGHT = 800;

        // AI Constants
        public const float MaxFallDistance = 60f;
        public const float MaxAITime = 50f;         // Time in ms to spend on EACH AIObject's pathfinding
        public const int AIUpdateInterval = 10;     // Do AI pathfinding once every this many ticks
    }
}
