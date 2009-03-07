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
    interface PhysicsObject
    {
        /*
         * The following are the standard gets and sets.
         * 
         * */

        Vector3 position
        {
            get;
            set;
        }

        Vector3 velocity
        {
            get;
            set;
        }

        Vector3 acceleration
        {
            get;
            set;
        }

        Vector3 force
        {
            get;
            set;
        }


        Vector3 centerOfMass
        {
            get;
            set;
        }

        Quaternion orientation
        {
            get;
            set;
        }

        Vector3 angularVelocity
        {
            get;
            set;
        }

        Vector3 angularAcceleration
        {
            get;
            set;
        }

        Vector3 torque
        {
            get;
            set;
        }
        

        Material material
        {
            get;
            set;
        }

        Boolean locked
        {
            get;
            set;
        }

        Boolean active
        {
            get;
            set;
        }

        /*
         * This method is used for applying a force to the
         * object at a given location.  From this, linear
         * force and torque will be calculated.
         * 
         * */

        void applyForce(float force, Vector3 location);

        /*
         * This method is used to update the position
         * and orientation of the object based on the
         * current force and torque acting upon it.
         * 
         * */

        void integrate();

        /*
         * Returns the list of collision regions that this object
         * currently has.
         * 
         * */

        List<CollisionRegion> collisionRegions
        {
            get;
        }

        /*
         * Used to append a collision on to the end of the list.
         * 
         * */

        void addCollision(CollisionRegion collision);

        /*
         * This method is used for any setup necessary before the physics runs.
         * 
         * */

        void prePhysics();

        /*
         * This method should be used to reset the collision regions
         * and force and torque after every iteration through the main
         * loop. Another other functionality used to clean up after the
         * physics has run should also be in this method.
         * 
         * */

        void postPhysics();
        
    }
}
