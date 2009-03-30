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

            // apply gravity to each object
            foreach (GameObject o in objects)
            {
                _gravity.applyForce(o);
            }

            // apply gravity to each object
            foreach (GameObject o in objects)
            {
                //o.updateBoundingBox();
            }

            // physics processing
            foreach (GameObject o in objects)
            {
               
                Boolean collision = false;
                // CHECK FOR COLLISIONS HERE

                foreach (GameObject testObject in objects)
                {
                    // don't check this object against itself
                    if (o != testObject && (o.m_model_type != ModelType.TERRAIN || testObject.m_model_type != ModelType.TERRAIN))
                    {
                        // collision detection here
                        collision = o.boundingBox.Intersects(testObject.boundingBox);

                        
                        if (collision)
                        {
                            
                            Vector3 currentDistanceApart = o.position - testObject.position;
                            Vector3 previousDistanceApart = o.previousPosition - testObject.previousPosition;

                            currentDistanceApart.X = Math.Abs(currentDistanceApart.X);
                            currentDistanceApart.Y = Math.Abs(currentDistanceApart.Y);
                            currentDistanceApart.Z = Math.Abs(currentDistanceApart.Z);

                            previousDistanceApart.X = Math.Abs(previousDistanceApart.X);
                            previousDistanceApart.Y = Math.Abs(previousDistanceApart.Y);
                            previousDistanceApart.Z = Math.Abs(previousDistanceApart.Z);

                            Vector3 temp = currentDistanceApart - previousDistanceApart;


                            if (temp.X < 0)
                            {
                                o._velocity.X = -.5f * o._velocity.X;
                                testObject._velocity.X = -.5f * testObject._velocity.X;

                                o._force.X = -o._force.X;
                                testObject._force.X = -testObject._force.X;

                                o._acceleration.X = 0;
                                testObject._acceleration.X = 0;
                            }

                            if (temp.Y < 0)
                            {
                                o._velocity.Y = -.5f * o._velocity.Y;
                                testObject._velocity.Y = -.5f * testObject._velocity.Y;

                                o._force.Y = -o._force.Y;
                                testObject._force.Y = -testObject._force.Y;

                                o._acceleration.Y = 0;
                                testObject._acceleration.Y = 0;
                            }

                            if (temp.Z < 0)
                            {
                                o._velocity.Z = -.5f * o._velocity.Z;
                                testObject._velocity.Z = -.5f * testObject._velocity.Z;

                                o._force.Z = -o._force.Z;
                                testObject._force.Z = -testObject._force.Z;

                                o._acceleration.Z = 0;
                                testObject._acceleration.Z = 0;
                            }

                            // figure out how to resolve the collision


                            break;
                        }
                    }
                }
                   
               
                o.integrate(timestep);
            }
        }

        #endregion
    }
}
