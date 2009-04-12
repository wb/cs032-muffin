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
        private int count;

        public Physics(Game game)
            : base(game)
        {
            _muffinGame = (MuffinGame)game;
            _gravity = new Gravity(-70.0f);

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
            //Console.WriteLine(gameTime.ElapsedGameTime.Milliseconds);

            // apply gravity to each object
            foreach (GameObject activeObject in _muffinGame.allObjects)
            {
                _gravity.applyForce(activeObject);
                activeObject.prePhysics();
            }


            /*
             * This is the physics response to and check for collisions.  activeObject is the object
             * we are currently testing and resolving.  Passive object the object which we are colliding
             * and resolving against.
             * */

            foreach (GameObject activeObject in _muffinGame.allObjects)
            {
                // only check active objects
                if (activeObject.locked)
                    continue;



                // integrate first (this will set future position)
                activeObject.integrate(((float)gameTime.ElapsedGameTime.TotalSeconds));

                Boolean collision = false;
                // CHECK FOR COLLISIONS HERE

                foreach (GameObject passiveObject in _muffinGame.allObjects)
                {
                    // don't check this object against itself
                    if (activeObject != passiveObject)
                    {
                        // collision detection here
                        collision = activeObject.boundingBox.Intersects(passiveObject.boundingBox);


                        if (collision)
                        {
                            float amountOfBounce = 0.2f;

                            Vector3 futureDistanceApart = activeObject.futureState.position - passiveObject.futureState.position;
                            Vector3 currentDistanceApart = activeObject.currentState.position - passiveObject.currentState.position;

                            futureDistanceApart.X = Math.Abs(futureDistanceApart.X);
                            futureDistanceApart.Y = Math.Abs(futureDistanceApart.Y);
                            futureDistanceApart.Z = Math.Abs(futureDistanceApart.Z);

                            currentDistanceApart.X = Math.Abs(currentDistanceApart.X);
                            currentDistanceApart.Y = Math.Abs(currentDistanceApart.Y);
                            currentDistanceApart.Z = Math.Abs(currentDistanceApart.Z);

                            Vector3 changeInFutureDistanceApart = futureDistanceApart - currentDistanceApart;

                            // resolve the collision
                            if (changeInFutureDistanceApart.X < 0)
                            {
                                activeObject.currentState.velocity = new Vector3(-amountOfBounce * activeObject.futureState.velocity.X, activeObject.futureState.velocity.Y, activeObject.futureState.velocity.Z);
                                passiveObject.currentState.velocity = new Vector3(-amountOfBounce * passiveObject.futureState.velocity.X, passiveObject.futureState.velocity.Y, passiveObject.futureState.velocity.Z);

                                activeObject.currentState.acceleration = new Vector3(0, activeObject.futureState.acceleration.Y, activeObject.futureState.acceleration.Z);
                                passiveObject.currentState.acceleration = new Vector3(0, passiveObject.futureState.acceleration.Y, passiveObject.futureState.acceleration.Z);
                            }

                            if (changeInFutureDistanceApart.Y < 0)
                            {
                                activeObject.currentState.velocity = new Vector3(activeObject.futureState.velocity.X, -amountOfBounce * activeObject.futureState.velocity.Y, activeObject.futureState.velocity.Z);
                                passiveObject.currentState.velocity = new Vector3(passiveObject.futureState.velocity.X, -amountOfBounce * passiveObject.futureState.velocity.Y, passiveObject.futureState.velocity.Z);

                                activeObject.currentState.acceleration = new Vector3(activeObject.futureState.acceleration.X, 0, activeObject.futureState.acceleration.Z);
                                passiveObject.currentState.acceleration = new Vector3(passiveObject.futureState.acceleration.X, 0, passiveObject.futureState.acceleration.Z);
                            }

                            if (changeInFutureDistanceApart.Z < 0)
                            {
                                activeObject.currentState.velocity = new Vector3(activeObject.futureState.velocity.X, activeObject.futureState.velocity.Y, -amountOfBounce * activeObject.futureState.velocity.Z);
                                passiveObject.currentState.velocity = new Vector3(passiveObject.futureState.velocity.X, passiveObject.futureState.velocity.Y, -amountOfBounce * passiveObject.futureState.velocity.Z);

                                activeObject.currentState.acceleration = new Vector3(activeObject.futureState.acceleration.X, activeObject.futureState.acceleration.Y, 0);
                                passiveObject.currentState.acceleration = new Vector3(passiveObject.futureState.acceleration.X, passiveObject.futureState.acceleration.Y, 0);
                            }

                            // we only want to process one collision for this timestep, so break
                            break;
                        }
                    }
                }

                if (!collision)
                    activeObject.postPhysics(true);
                else
                    activeObject.postPhysics(false);
            }

            /* In addition to checking for collisions on all objects, we need to check for collisions
             * on objects that have been moved by forces outside of the realm of physics (such as input
             * from a controller, AI, etc.  Therefore, this code can be run on just objects of type human
             * player and AI.
             * */

            // first, check all human players
            foreach (GameObject activeObject in _muffinGame.allPlayer)
            {
                foreach (GameObject passiveObject in _muffinGame.allObjects)
                {
                    if (activeObject == passiveObject)
                        continue;

                    BoundingBox activeBoundingBox = activeObject.getCurrentBoundingBox();
                    BoundingBox passiveBoundingBox = passiveObject.getCurrentBoundingBox();

                    // check to see if the bounding boxes are intersecting
                    if (activeBoundingBox.Intersects(passiveBoundingBox))
                    {

                        // compute the centers of these bounding boxes
                        Vector3 activeBoundingBoxCenter = (activeBoundingBox.Min + activeBoundingBox.Max) / 2.0f;
                        Vector3 passiveBoundingBoxCenter = (passiveBoundingBox.Min + passiveBoundingBox.Max) / 2.0f;

                        // relative positions (for determining which side of passiveBoundingBox activeBoundingBox is on
                        Vector3 relativePositions = activeBoundingBoxCenter - passiveBoundingBoxCenter;

                        // compute their edge lengths
                        Vector3 activeBoundingBoxEdgeLengths = activeBoundingBox.Max - activeBoundingBox.Min;
                        Vector3 passiveBoundingBoxEdgeLengths = passiveBoundingBox.Max - passiveBoundingBox.Min;

                        // now compute the distance between them
                        Vector3 distanceBetween = VectorAbs(activeBoundingBoxCenter - passiveBoundingBoxCenter);
                        Vector3 allowedDistanceBetween = VectorAbs(activeBoundingBoxEdgeLengths + passiveBoundingBoxEdgeLengths) / 2.0f;

                        // now check for penetration of these two bounding boxes
                        Vector3 penetration = distanceBetween - allowedDistanceBetween;

                        // now, all negative values are penetrations, so we can resolve them
                        Vector3 correction;
                        correction.X = (penetration.X < 0 ? penetration.X : 0);
                        correction.Y = (penetration.Y < 0 ? penetration.Y : 0);
                        correction.Z = (penetration.Z <= 0 ? penetration.Z : 0);

                        // now we want to correct the smallest absolute value of these
                        Vector3 tempCorrect = VectorAbs(correction);


                        /*
                         * If the other object is locked (like terrain is), then we can only work with the activeObject
                         * to resolve this conflict.
                         * */

                        if (passiveObject.locked)
                        {
                            // if x is the smallest, fix it in the x direction
                            if (tempCorrect.X < tempCorrect.Y && tempCorrect.X < tempCorrect.Z)
                            {
                                // need to see which size of the passiveObject the activeObject is on
                                int sign = (relativePositions.X > 0 ? -1 : 1);

                                activeObject.futureState.position = activeObject.futureState.position + sign * new Vector3(correction.X, 0, 0);
                                activeObject.currentState.position = activeObject.currentState.position + sign * new Vector3(correction.X, 0, 0);
                            }
                            // if y is the smallest, fix it in the y direction
                            else if (tempCorrect.Y < tempCorrect.X && tempCorrect.Y < tempCorrect.Z)
                            {
                                // always force y correction to be upwards
                                //activeObject.futureState.position = activeObject.futureState.position + new Vector3(0, Math.Abs(correction.Y), 0);
                                //activeObject.currentState.position = activeObject.currentState.position + new Vector3(0, Math.Abs(correction.Y), 0);
                            }
                            // otherwise, fix it in the z direction
                            else if (tempCorrect.Z < tempCorrect.X && tempCorrect.Z < tempCorrect.Y)
                            {
                                // need to see which size of the passiveObject the activeObject is on
                                int sign = (relativePositions.Z > 0 ? -1 : 1);

                                activeObject.futureState.position = activeObject.futureState.position + sign * new Vector3(0, 0, correction.Z);
                                activeObject.currentState.position = activeObject.currentState.position + sign * new Vector3(0, 0, correction.Z);
                            }
                        }

                        else
                        {
                            // if x is the smallest, fix it in the x direction
                            if (tempCorrect.X < tempCorrect.Y && tempCorrect.X < tempCorrect.Z)
                            {
                                // need to see which size of the activeObject the passiveObject is on
                                int sign = (relativePositions.X > 0 ? 1 : -1);

                                passiveObject.futureState.position = passiveObject.futureState.position + sign * new Vector3(correction.X, 0, 0);
                                passiveObject.currentState.position = passiveObject.currentState.position + sign * new Vector3(correction.X, 0, 0);
                            }
                            // if y is the smallest, fix it in the y direction
                            else if (tempCorrect.Y < tempCorrect.X && tempCorrect.Y < tempCorrect.Z)
                            {
                                // always force y correction to be upwards
                                //passiveObject.futureState.position = passiveObject.futureState.position + new Vector3(0, Math.Abs(correction.Y), 0);
                                //passiveObject.currentState.position = passiveObject.currentState.position + new Vector3(0, Math.Abs(correction.Y), 0);
                            }
                            // otherwise, fix it in the z direction
                            else if (tempCorrect.Z < tempCorrect.X && tempCorrect.Z < tempCorrect.Y)
                            {
                                // need to see which size of the passiveObject the activeObject is on
                                int sign = (relativePositions.Z > 0 ? 1 : -1);

                                passiveObject.futureState.position = passiveObject.futureState.position + sign * new Vector3(0, 0, correction.Z);
                                passiveObject.currentState.position = passiveObject.currentState.position + sign * new Vector3(0, 0, correction.Z);
                            }
                        }


                    }
                }

            }
            base.Update(gameTime);
        }

        private Vector3 VectorAbs(Vector3 vector)
        {
            Vector3 absVector;

            absVector.X = Math.Abs(vector.X);
            absVector.Y = Math.Abs(vector.Y);
            absVector.Z = Math.Abs(vector.Z);

            return absVector;
        }
    }
}