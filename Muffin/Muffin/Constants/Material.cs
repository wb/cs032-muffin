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
    /*
     * This class contains the values neccessary for an object to respond
     * properly to sliding (friction), starting (kinetic friction),
     * and bouncing (restitution).
     * */

    class Material
    {
        private float _staticFriction, _kineticFriction, _restitution;

        public Material(float staticFriction, float kineticFriction, float restitution)
        {
            _staticFriction = staticFriction;
            _kineticFriction = kineticFriction;
            _restitution = restitution;
        }

        float staticFriction
        {
            get { return _staticFriction; }
            set { _staticFriction = value; }
        }

        float kineticFriction
        {
            get { return _kineticFriction; }
            set { _kineticFriction = value; }
        }

        float restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }
    }


}
