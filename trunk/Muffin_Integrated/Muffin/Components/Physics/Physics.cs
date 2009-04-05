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
using Definitions;

namespace Muffin.Components.Physics
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Physics : Microsoft.Xna.Framework.GameComponent
    {

        private ForceGenerator _gravity;
        private MuffinGame _muffinGame;

        public Physics(Game game)
            : base(game)
        {
            _muffinGame = (MuffinGame)game;
            _gravity = new Gravity(-9.8f);

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // apply gravity to each object
            foreach (GameObject o in _muffinGame.allObjects)
            {
                _gravity.applyForce(o);
            }


            // physics processing
            foreach (GameObject o in _muffinGame.allObjects)
            {

                Boolean collision = false;
                // CHECK FOR COLLISIONS HERE

                foreach (GameObject testObject in _muffinGame.allObjects)
                {
                    // don't check this object against itself
                    if (o != testObject && (o.modelType != ModelType.TERRAIN || testObject.modelType != ModelType.TERRAIN))
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
                                o.velocity = new Vector3(-.5f * o.velocity.X, o.velocity.Y, o.velocity.Z);
                                testObject.velocity = new Vector3(-.5f * testObject.velocity.X, testObject.velocity.Y, testObject.velocity.Z);

                                o.force = new Vector3(-o.force.X, o.force.Y, o.force.Z);
                                testObject.force = new Vector3(-testObject.force.X, testObject.force.Y, testObject.force.Z);

                                o.acceleration = new Vector3(0, o.acceleration.Y, o.acceleration.Z);
                                testObject.acceleration = new Vector3(0, testObject.acceleration.Y, testObject.acceleration.Z);
                            }

                            if (temp.Y < 0)
                            {
                                o.velocity = new Vector3(o.velocity.X, -.5f * o.velocity.Y, o.velocity.Z);
                                testObject.velocity = new Vector3(testObject.velocity.X, -.5f * testObject.velocity.Y, testObject.velocity.Z);

                                o.force = new Vector3(o.force.X, -o.force.Y, o.force.Z);
                                testObject.force = new Vector3(testObject.force.X, -testObject.force.Y, testObject.force.Z);

                                o.acceleration = new Vector3(o.acceleration.X, 0, o.acceleration.Z);
                                testObject.acceleration = new Vector3(testObject.acceleration.X, 0, testObject.acceleration.Z);
                            }

                            if (temp.Z < 0)
                            {
                                o.velocity = new Vector3(o.velocity.X, o.velocity.Y, -.5f * o.velocity.Z);
                                testObject.velocity = new Vector3(testObject.velocity.X, testObject.velocity.Y, -.5f * testObject.velocity.Z);

                                o.force = new Vector3(o.force.X, o.force.Y, -o.force.Z);
                                testObject.force = new Vector3(testObject.force.X, testObject.force.Y, -testObject.force.Z);

                                o.acceleration = new Vector3(o.acceleration.X, o.acceleration.Y, 0);
                                testObject.acceleration = new Vector3(testObject.acceleration.X, testObject.acceleration.Y, 0);
                            }

                            // figure out how to resolve the collision


                            break;
                        }
                    }
                }


                o.integrate((float) gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }
    }
}