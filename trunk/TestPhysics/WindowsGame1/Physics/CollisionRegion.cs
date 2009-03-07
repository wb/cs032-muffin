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
    interface CollisionRegion
    {
        /*
         * Three interfaces should be as follows:
         * 
         * CollisionRegion(GameObject obj, Vector3 one);
         * CollisionRegion(GameObject obj, Vector3 one, Vector3 two);
         * CollisionRegion(GameObject obj, Vector3 one, Vector3 two, Vector3 three);
         * 
         * */
        
        /*
         * Get the type of collision.
         * 
         * */

        CollisionType collisionType();
        
        /*
         * Methods used for getting the points involved
         * in the collision.
         * 
         * */

        Vector3[] getPointsOfCollsion();
        Vector3 getPointOfCollision(int index);
        
        /*
         * Returns the normal vector for this collision
         * 
         * */

        Vector3 normalVector();
    }
}
