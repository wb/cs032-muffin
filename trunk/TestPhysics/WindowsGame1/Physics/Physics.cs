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
    class Physics : PhysicsEngine
    {
        #region PhysicsEngine Members

        private ForceGenerator _gravity;

        public Physics()
        {
            _gravity = new Gravity(-9.8f);

        }
        public void update(List<GameObject> objects, float timestep)
        {

            // physics processing
            foreach (GameObject o in objects)
            {
                // apply gravity
                _gravity.applyForce(o);
               
                // integrate
                o.integrate(timestep);
            }
        }

        #endregion
    }
}
