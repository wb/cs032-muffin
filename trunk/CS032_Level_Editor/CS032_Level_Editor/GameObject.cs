using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.IO;

namespace CS032_Level_Editor
{

    class GameObject
    {
        Model _model;
        Vector3 _position, _velocity, _acceleration, _force, _centerOfMass, _rotation;
        double _mass, _friction;

        public GameObject()
        {
        }


        // resets the force at the end of each loop
        public void resetForce()
        {
            _force = Vector3.Zero;
        }



        /**
         * All of the getters and setters.
         * 
         **/

        public Model model
        {
            get { return _model; }
            set { _model = value; }
        }

        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public Vector3 acceleration
        {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public Vector3 force
        {
            get { return _force; }
            set { _force = value; }
        }

        public Vector3 centerOfMass
        {
            get { return _centerOfMass; }
            set { _centerOfMass = value; }
        }

        public Vector3 rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public double mass
        {
            get { return _mass; }
            set { _mass = value; }
        }

        public double friction
        {
            get { return _friction; }
            set { _friction = value; }
        }

    }
}
