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
    /*
     * A ForceGenerator is anything that can apply
     * a force to an object.  Examples of this are
     * gravity, friction, collision response, and
     * air resistance.
     * 
     * */

    interface ForceGenerator
    {
        void applyForce(PhysicsObject obj);
    }
}
