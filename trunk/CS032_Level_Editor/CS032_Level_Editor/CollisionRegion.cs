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
using System.Xml;
using System.IO;
using System.Text;
namespace CS032_Level_Editor
{
    /**
     * This class represents a region of collision between two objects.  Each object
     * has a list of these CollisionRegions.  Therefore, it is only necessary to pass
     * in the other object, as well as the point, line, or plane (in the form of three
     * points) to the constructor.
     * 
     * This class provides access to the object and points.  It also performs some
     * calculations, such as computing the normal vector if the region is a plane.
     * 
     * */

    public enum CollisionType
    {
        POINT,
        LINE,
        PLANE
    }
    class CollisionRegion
    {
        private CollisionType _collisionType;
        private Vector3[] _points;

        public CollisionRegion(GameObject o, Vector3 one)
        {
            _collisionType = CollisionType.POINT;
            _points = new Vector3[1] { one };
        }

        public CollisionRegion(GameObject o, Vector3 one, Vector3 two)
        {
            _collisionType = CollisionType.LINE;
            _points = new Vector3[2] { one, two };
        }

        public CollisionRegion(GameObject o, Vector3 one, Vector3 two, Vector3 three)
        {
            _collisionType = CollisionType.PLANE;
            _points = new Vector3[] { one, two, three };
        }

        public CollisionType collisionType()
        {
            return _collisionType;

        }

        public Vector3[] getPoints()
        {
            return _points;
        }

        public Vector3 getPoint(int index)
        {
            if (index >= 0 && index < _points.Length)
                return _points[index];
            else
                throw new ArgumentOutOfRangeException("Invalid index into array.");
        }

        public Vector3 normalVector()
        {
            // a normal can only be computed if there is a plane
            if (_collisionType == CollisionType.PLANE)
            {
                // compute two vectors in the plane
                Vector3 vectorA = _points[0] - _points[1];
                Vector3 vectorB = _points[2] - _points[1];

                // now, find the cross product of these two vectors
                Vector3 normal = Vector3.Cross(vectorA, vectorB);

                // and normalize it
                normal.Normalize();

                // return the newly computed normal
                return normal;

            }
            else
            {
                // throw an exception or something
                return Vector3.Zero;
            }
        }

    }
}
