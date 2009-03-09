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
    class Gravity : ForceGenerator
    {
        // this field stores the acceleration due to gravity.
        // while the standard value, 9.81 will work, it might
        // be more fun to set it higher or lower depending on
        // the effect we want to achieve.

        private float _acceleration;

        public Gravity(float acceleration)
        {
            _acceleration = acceleration;
        }

        public void applyForce(PhysicsObject obj)
        {
            obj.applyForce(new Vector3(0, _acceleration * obj.mass, 0) , obj.centerOfMass);
        }
    }
}
