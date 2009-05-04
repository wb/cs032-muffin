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
            _gravity = new Gravity(-70.0f);

        }

        public override void Initialize()
        {

            base.Initialize();

        }

        public override void Update(GameTime gameTime)
        {
            // spin the star!



            // if paused, return
            if (_muffinGame.paused)
                return;


            // print out approximate FPS
            //Console.WriteLine(1000.0f / gameTime.ElapsedGameTime.Milliseconds);

            // apply gravity to each object
            foreach (GameObject activeObject in _muffinGame.allObjects)
            {

                _gravity.applyForce(activeObject);
                activeObject.prePhysics(_muffinGame);
            }


            /*
             * This is the physics response to and check for collisions.  activeObject is the object
             * we are currently testing and resolving.  Passive object the object which we are colliding
             * and resolving against.
             * */

            foreach (GameObject activeObject in _muffinGame.allObjects)
            {
                // only check active objects (not locked, not collectable)
                if (activeObject.locked || activeObject is CollectableObject)
                    continue;

                // integrate first (this will set future position)
                activeObject.integrate(((float)gameTime.ElapsedGameTime.TotalSeconds));

                Boolean collision = false;
                // CHECK FOR COLLISIONS HERE

                Boolean breaks = false;

                List<List<GameObject>> allCheck = _muffinGame.grid.getList((int)activeObject.index.X,
                                                                           (int)activeObject.index.Y,
                                                                           (int)activeObject.index.Z);

                foreach (List<GameObject> l in allCheck)
                {
                    if (l.Count != 0)
                        foreach (GameObject passiveObject in l)
                        {
                            // don't check this object against itself
                            if (activeObject != passiveObject && !(passiveObject is CollectableObject))
                            {
                                // get the bounding boxes
                                BoundingBox activeBoundingBox = activeObject.boundingBox;
                                BoundingBox passiveBoundingBox = passiveObject.boundingBox;

                                // check to see if these two boxes are colliding
                                collision = activeBoundingBox.Intersects(passiveBoundingBox);


                                if (collision)
                                {

                                    activeObject.jumpCount = 0;

                                    // change how much bounce collisions give
                                    float amountOfBounce = 0.2f;

                                    // this makes it easier for people to jump onto moving boxes
                                    if(passiveObject is AIRailObject)
                                        amountOfBounce = -0.2f;

                                    Vector3 futureDistanceApart = activeObject.futureState.position - passiveObject.futureState.position;
                                    Vector3 currentDistanceApart = activeObject.currentState.position - passiveObject.currentState.position;

                                    futureDistanceApart.X = Math.Abs(futureDistanceApart.X);
                                    futureDistanceApart.Y = Math.Abs(futureDistanceApart.Y);
                                    futureDistanceApart.Z = Math.Abs(futureDistanceApart.Z);

                                    currentDistanceApart.X = Math.Abs(currentDistanceApart.X);
                                    currentDistanceApart.Y = Math.Abs(currentDistanceApart.Y);
                                    currentDistanceApart.Z = Math.Abs(currentDistanceApart.Z);

                                    Vector3 changeInFutureDistanceApart = futureDistanceApart - currentDistanceApart;

                                    // tune the sensitivity of collisions here
                                    float collisionEpsilon = -0.1f;

                                    // collision should take into account the masses of both objects
                                    float totalMass = (passiveObject.mass + activeObject.mass);

                                    // move it relative to mass (locked objects return infinite mass)
                                    float passiveFactor = activeObject.mass / totalMass;
                                    float activeFactor = passiveObject.mass / totalMass;

                                    Vector3 totalVelocity = activeObject.futureState.velocity + passiveObject.futureState.velocity;

                                    Vector3 activeVelocity = activeFactor * totalVelocity;
                                    Vector3 passiveVelocity = passiveFactor * totalVelocity;

                                    /*
                                     * In order to ensure that rail objects carry player objects, we must remove all collision in the x and y direction if and only if
                                     * the player object is above the rail object.
                                     * 
                                     * */

                                    float twoWithError = 2.1f; // we must divide by a little over two to account to ensure they appear to be touching even if slightly apart

                                    // if we are above the rail object
                                    if (activeObject is AIRailObject && passiveObject is PlayerObject && (passiveObject.position.Y - activeObject.position.Y >= (passiveObject.dimensions.Y + activeObject.dimensions.Y) / twoWithError))
                                    {
                                        // make sure they collide only in the y direction
                                        changeInFutureDistanceApart.Y = -float.MaxValue;
                                    }
                                    // the other case for being above
                                    else if (activeObject is PlayerObject && passiveObject is AIRailObject && (activeObject.position.Y - passiveObject.position.Y >= (activeObject.dimensions.Y + passiveObject.dimensions.Y) / twoWithError))
                                    {
                                        // make sure they collide only in the y direction
                                        changeInFutureDistanceApart.Y = -float.MaxValue;
                                    }
                                    
                                    if (changeInFutureDistanceApart.X < collisionEpsilon && changeInFutureDistanceApart.X < changeInFutureDistanceApart.Y && changeInFutureDistanceApart.X < changeInFutureDistanceApart.Z)
                                    {
                                        activeObject.currentState.velocity = new Vector3(-amountOfBounce * activeVelocity.X, activeObject.futureState.velocity.Y, activeObject.futureState.velocity.Z);
                                        passiveObject.currentState.velocity = new Vector3(-amountOfBounce * passiveVelocity.X, passiveObject.futureState.velocity.Y, passiveObject.futureState.velocity.Z);

                                        activeObject.currentState.acceleration = new Vector3(0, activeObject.futureState.acceleration.Y, activeObject.futureState.acceleration.Z);
                                        passiveObject.currentState.acceleration = new Vector3(0, passiveObject.futureState.acceleration.Y, passiveObject.futureState.acceleration.Z);
                                    }

                                    if (changeInFutureDistanceApart.Y < collisionEpsilon && changeInFutureDistanceApart.Y < changeInFutureDistanceApart.X && changeInFutureDistanceApart.Y < changeInFutureDistanceApart.Z)
                                    {
                                        activeObject.currentState.velocity = new Vector3(activeObject.futureState.velocity.X, -amountOfBounce * activeVelocity.Y, activeObject.futureState.velocity.Z);
                                        passiveObject.currentState.velocity = new Vector3(passiveObject.futureState.velocity.X, -amountOfBounce * passiveVelocity.Y, passiveObject.futureState.velocity.Z);
                                        activeObject.currentState.acceleration = new Vector3(activeObject.futureState.acceleration.X, 0, activeObject.futureState.acceleration.Z);
                                        passiveObject.currentState.acceleration = new Vector3(passiveObject.futureState.acceleration.X, 0, passiveObject.futureState.acceleration.Z);
                                    }

                                    if (changeInFutureDistanceApart.Z < collisionEpsilon && changeInFutureDistanceApart.Z < changeInFutureDistanceApart.X && changeInFutureDistanceApart.Z < changeInFutureDistanceApart.Y)
                                    {
                                        activeObject.currentState.velocity = new Vector3(activeObject.futureState.velocity.X, activeObject.futureState.velocity.Y, -amountOfBounce * activeVelocity.Z);
                                        passiveObject.currentState.velocity = new Vector3(passiveObject.futureState.velocity.X, passiveObject.futureState.velocity.Y, -amountOfBounce * passiveVelocity.Z);

                                        activeObject.currentState.acceleration = new Vector3(activeObject.futureState.acceleration.X, activeObject.futureState.acceleration.Y, 0);
                                        passiveObject.currentState.acceleration = new Vector3(passiveObject.futureState.acceleration.X, passiveObject.futureState.acceleration.Y, 0);
                                    }



                                    // we only want to process one collision for this timestep, so break
                                    breaks = true;
                                    break;
                                }
                                if (breaks) break;
                            }
                            if (breaks) break;
                        }
                    if (breaks) break;
                }

                if (!collision)
                {
                    activeObject.postPhysics(true, _muffinGame);
                }
                else
                    activeObject.postPhysics(false, _muffinGame);
            }

            /* In addition to checking for collisions on all objects, we need to check for collisions
             * on objects that have been moved by forces outside of the realm of physics (such as input
             * from a controller, AI, etc.  Therefore, this code can be run on just objects of type human
             * player and AI.
             * */

            // first, check all human players
            foreach (GameObject activeObject in _muffinGame.allObjects)
            {
                // skip locked and collectable objects
                if (activeObject.locked || activeObject is CollectableObject)
                    continue;
                List<List<GameObject>> allCheck = _muffinGame.grid.getList((int)activeObject.index.X,
                                                           (int)activeObject.index.Y,
                                                           (int)activeObject.index.Z);

                foreach (List<GameObject> l in allCheck)
                {
                    if (l.Count != 0)
                        foreach (GameObject passiveObject in l)
                        {
                            if (activeObject == passiveObject || passiveObject is CollectableObject)
                                continue;

                            BoundingBox activeBoundingBox = activeObject.getCurrentBoundingBoxEnlarged();
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

                                // multiply by a small factor to make sure it moves slightly more than it has to to avoid incorrectly resolving a collision

                                if (activeObject.modelName == ModelName.PLAYER || passiveObject.modelName == ModelName.PLAYER)
                                    correction *= 1.35f;
                                else
                                    correction *= 1.25f;

                                // now we want to correct the smallest absolute value of these
                                Vector3 tempCorrect = VectorAbs(correction);

                                /*
                                 * This code uses the relative weight of the two objects to determine how much to move them.
                                 * */

                                float totalMass = (passiveObject.mass + activeObject.mass);

                                // move it relative to mass (locked objects return infinite mass)
                                float passiveFactor = activeObject.mass / totalMass;
                                float activeFactor = passiveObject.mass / totalMass;

                                /*
                                 * Rail Objects, when they come in contact with the bottom of the player, are supposed to move the player with them.
                                 * 
                                 * */

                                float twoWithError = 2.1f; // we must divide by a little over two to account to ensure they appear to be touching even if slightly apart

                                // if we are above the rail object
                                if (activeObject is AIRailObject && passiveObject is PlayerObject && (passiveObject.position.Y - activeObject.position.Y >= (passiveObject.dimensions.Y + activeObject.dimensions.Y) / twoWithError))
                                {
                                    // calculate how much the rail object has moved since the last update
                                    Vector3 amountMoved = activeObject.currentState.position - activeObject.previousState.position;
                                    // and add that to the player object
                                    passiveObject.currentState.position = passiveObject.currentState.position + amountMoved;
                                }
                                // the other case for being above
                                else if (activeObject is PlayerObject && passiveObject is AIRailObject && (activeObject.position.Y - passiveObject.position.Y >= (activeObject.dimensions.Y + passiveObject.dimensions.Y) / twoWithError))
                                {
                                    // we need to catch this case, but do nothing
                                }

                                /*
                                 * Eeverything else should be handled the normal way.
                                 * 
                                 * */
                                else
                                {
                                    // if x is the smallest, fix it in the x direction
                                    if (tempCorrect.X < tempCorrect.Y && tempCorrect.X < tempCorrect.Z)
                                    {
                                        // need to see which size of the activeObject the passiveObject is on
                                        int sign = (relativePositions.X > 0 ? 1 : -1);

                                        // move the passive object
                                        passiveObject.futureState.position = passiveObject.futureState.position + sign * passiveFactor * new Vector3(correction.X, 0, 0);
                                        passiveObject.currentState.position = passiveObject.currentState.position + sign * passiveFactor * new Vector3(correction.X, 0, 0);

                                        // attempt at friction
                                        //passiveObject.controlInput(100 * sign * passiveFactor * new Vector2(correction.X, 0), false);

                                        // move the active object
                                        activeObject.futureState.position = activeObject.futureState.position - sign * activeFactor * new Vector3(correction.X, 0, 0);
                                        activeObject.currentState.position = activeObject.currentState.position - sign * activeFactor * new Vector3(correction.X, 0, 0);

                                        // attempt at friction
                                        //activeObject.controlInput(-100 * sign * activeFactor * new Vector2(correction.X, 0), false);


                                    }
                                    // if y is the smallest, fix it in the y direction
                                    else if (tempCorrect.Y < tempCorrect.X && tempCorrect.Y < tempCorrect.Z)
                                    {

                                        // whichever one is higher moves.  the other stays still
                                        if (relativePositions.Y > 0)
                                        {
                                            // if the active object is higher, move it
                                            activeObject.futureState.position = activeObject.futureState.position - new Vector3(0, correction.Y, 0);
                                            activeObject.currentState.position = activeObject.currentState.position - new Vector3(0, correction.Y, 0);
                                        }
                                        // otherwise the passive object is higher, so move it
                                        else
                                        {
                                            passiveObject.futureState.position = passiveObject.futureState.position - new Vector3(0, correction.Y, 0);
                                            passiveObject.currentState.position = passiveObject.currentState.position - new Vector3(0, correction.Y, 0);
                                        }
                                    }
                                    // otherwise, fix it in the z direction
                                    else if (tempCorrect.Z < tempCorrect.X && tempCorrect.Z < tempCorrect.Y)
                                    {
                                        // need to see which size of the passiveObject the activeObject is on
                                        int sign = (relativePositions.Z > 0 ? 1 : -1);

                                        // move the passive object
                                        passiveObject.futureState.position = passiveObject.futureState.position + sign * passiveFactor * new Vector3(0, 0, correction.Z);
                                        passiveObject.currentState.position = passiveObject.currentState.position + sign * passiveFactor * new Vector3(0, 0, correction.Z);

                                        // attempt at friction
                                        //passiveObject.controlInput(100 * sign * passiveFactor * new Vector2(0, correction.Z), false);

                                        // move the active object
                                        activeObject.futureState.position = activeObject.futureState.position - sign * activeFactor * new Vector3(0, 0, correction.Z);
                                        activeObject.currentState.position = activeObject.currentState.position - sign * activeFactor * new Vector3(0, 0, correction.Z);

                                        // attempt at friction
                                        //activeObject.controlInput(-100 * sign * activeFactor * new Vector2(0, correction.Z), false);


                                    }
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