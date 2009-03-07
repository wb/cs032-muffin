using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using WindowsGame1;
using System.IO;


namespace Physics
{
    interface PhysicsEngine
    {
        /*
         * This is the only external method of this class.  The
         * physics engine simply needs a reference to all objects
         * in the game, and the duration of the current timestep.
         * 
         * */

        void update(List<PhysicsObject> objects, float timestep);
    }
}
