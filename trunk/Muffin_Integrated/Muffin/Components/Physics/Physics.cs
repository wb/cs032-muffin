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
                // integrate first (this will set future position)
                o.integrate(((float)gameTime.ElapsedGameTime.TotalSeconds));

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
                            float amountOfBounce = 0.2f;

                            Vector3 currentDistanceApart = o.futureState.position - testObject.futureState.position;
                            Vector3 previousDistanceApart = o.currentState.position - testObject.currentState.position;

                            currentDistanceApart.X = Math.Abs(currentDistanceApart.X);
                            currentDistanceApart.Y = Math.Abs(currentDistanceApart.Y);
                            currentDistanceApart.Z = Math.Abs(currentDistanceApart.Z);

                            previousDistanceApart.X = Math.Abs(previousDistanceApart.X);
                            previousDistanceApart.Y = Math.Abs(previousDistanceApart.Y);
                            previousDistanceApart.Z = Math.Abs(previousDistanceApart.Z);

                            Vector3 temp = currentDistanceApart - previousDistanceApart;


                            if (temp.X < 0)
                            {
                                o.futureState.velocity = new Vector3(-amountOfBounce * o.futureState.velocity.X, o.futureState.velocity.Y, o.futureState.velocity.Z);
                                testObject.futureState.velocity = new Vector3(-amountOfBounce * testObject.futureState.velocity.X, testObject.futureState.velocity.Y, testObject.futureState.velocity.Z);

                                o.futureState.acceleration = new Vector3(0, o.futureState.acceleration.Y, o.futureState.acceleration.Z);
                                testObject.futureState.acceleration = new Vector3(0, testObject.futureState.acceleration.Y, testObject.futureState.acceleration.Z);
                            }

                            if (temp.Y < 0)
                            {
                                o.futureState.velocity = new Vector3(o.futureState.velocity.X, -amountOfBounce * o.futureState.velocity.Y, o.futureState.velocity.Z);
                                testObject.futureState.velocity = new Vector3(testObject.futureState.velocity.X, -amountOfBounce * testObject.futureState.velocity.Y, testObject.futureState.velocity.Z);

                                o.futureState.acceleration = new Vector3(o.futureState.acceleration.X, 0, o.futureState.acceleration.Z);
                                testObject.futureState.acceleration = new Vector3(testObject.futureState.acceleration.X, 0, testObject.futureState.acceleration.Z);
                            }

                            if (temp.Z < 0)
                            {
                                o.futureState.velocity = new Vector3(o.futureState.velocity.X, o.futureState.velocity.Y, -amountOfBounce * o.futureState.velocity.Z);
                                testObject.futureState.velocity = new Vector3(testObject.futureState.velocity.X, testObject.futureState.velocity.Y, -amountOfBounce * testObject.futureState.velocity.Z);

                                o.futureState.acceleration = new Vector3(o.futureState.acceleration.X, o.futureState.acceleration.Y, 0);
                                testObject.futureState.acceleration = new Vector3(testObject.futureState.acceleration.X, testObject.futureState.acceleration.Y, 0);
                            }

                            // figure out how to resolve the collision


                            break;
                        }
                    }
                }

                if (!collision)
                    o.postPhysics(true);
                else
                    o.postPhysics(true);
            }




            base.Update(gameTime);
        }
    }
}