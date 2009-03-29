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

namespace Collision_Library
{
    public class CollisionMesh
    {
        //internal variables for the class
        
        //vertices of the model
        public Vector3[] Vertices;

        //index of the triangle
        public byte[] Indices;

        //index of the edges
        public byte[] IndicesOfEdges;

        //relative location
        public Vector3 Position;
        public Vector3 Rotation;

        public Matrix WorldMatrix;
        
        //check if changes occur
        public bool isChanged;

        //constant used for floating point error during collisoin check
        static readonly float FLOAT_ERROR = 0.99f;

        //constructor class
        public CollisionMesh(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
            isChanged = true;
            RefreshMatrix();
        }

        //recalculates the toworld matrix when it moves
        public void RefreshMatrix()
        {
            if (isChanged)
            {
                WorldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateTranslation(Position);
                isChanged = false;
            }
        }

        //box primitive model
        /**
         * This is the basic cube model primitive. The values a,b, and c are lenght,width and height. The method
         * then calculates the vertices, the triangulation, the edge indices and other things necessary for the model.
         * */
        public static CollisionMesh BoxPrimitive(float a, float b, float c, Vector3 position, Vector3 rotation)
        {
            CollisionMesh collision = new CollisionMesh(position, rotation);

            //Defining the vertices
            #region Vertices
            collision.Vertices = new Vector3[8];

            collision.Vertices[0].X = -a / 2;
            collision.Vertices[0].Y = b / 2;
            collision.Vertices[0].Z = c / 2;

            collision.Vertices[1].X = a / 2;
            collision.Vertices[1].Y = b / 2;
            collision.Vertices[1].Z = c / 2;

            collision.Vertices[2].X = a / 2;
            collision.Vertices[2].Y = -b / 2;
            collision.Vertices[2].Z = c / 2;

            collision.Vertices[3].X = -a / 2;
            collision.Vertices[3].Y = -b / 2;
            collision.Vertices[3].Z = c / 2;

            collision.Vertices[4].X = -a / 2;
            collision.Vertices[4].Y = b / 2;
            collision.Vertices[4].Z = -c / 2;

            collision.Vertices[5].X = a / 2;
            collision.Vertices[5].Y = b / 2;
            collision.Vertices[5].Z = -c / 2;

            collision.Vertices[6].X = a / 2;
            collision.Vertices[6].Y = -b / 2;
            collision.Vertices[6].Z = -c / 2;

            collision.Vertices[7].X = -a / 2;
            collision.Vertices[7].Y = -b / 2;
            collision.Vertices[7].Z = -c / 2;
            #endregion

            //defining the index of the vertices used for triangulation clockwise.
            #region Indices
            collision.Indices = new byte[36];

            collision.Indices[0] = 0;
            collision.Indices[1] = 1;
            collision.Indices[2] = 2;

            collision.Indices[3] = 2;
            collision.Indices[4] = 3;
            collision.Indices[5] = 0;

            collision.Indices[6] = 0;
            collision.Indices[7] = 4;
            collision.Indices[8] = 5;

            collision.Indices[9] = 5;
            collision.Indices[10] = 1;
            collision.Indices[11] = 0;

            collision.Indices[12] = 2;
            collision.Indices[13] = 1;
            collision.Indices[14] = 5;

            collision.Indices[15] = 5;
            collision.Indices[16] = 6;
            collision.Indices[17] = 2;

            collision.Indices[18] = 3;
            collision.Indices[19] = 2;
            collision.Indices[20] = 6;

            collision.Indices[21] = 6;
            collision.Indices[22] = 7;
            collision.Indices[23] = 3;

            collision.Indices[24] = 0;
            collision.Indices[25] = 3;
            collision.Indices[26] = 7;

            collision.Indices[27] = 7;
            collision.Indices[28] = 4;
            collision.Indices[29] = 0;

            collision.Indices[30] = 7;
            collision.Indices[31] = 6;
            collision.Indices[32] = 5;

            collision.Indices[33] = 5;
            collision.Indices[34] = 4;
            collision.Indices[35] = 7;
            #endregion

            //The indices for the edges on the cube
            #region IndicesOfEdges
            collision.IndicesOfEdges = new byte[24];
            collision.IndicesOfEdges[0] = 0;
            collision.IndicesOfEdges[1] = 1;

            collision.IndicesOfEdges[2] = 1;
            collision.IndicesOfEdges[3] = 2;

            collision.IndicesOfEdges[4] = 2;
            collision.IndicesOfEdges[5] = 3;

            collision.IndicesOfEdges[6] = 3;
            collision.IndicesOfEdges[7] = 0;

            collision.IndicesOfEdges[8] = 4;
            collision.IndicesOfEdges[9] = 5;

            collision.IndicesOfEdges[10] = 5;
            collision.IndicesOfEdges[11] = 6;

            collision.IndicesOfEdges[12] = 6;
            collision.IndicesOfEdges[13] = 7;

            collision.IndicesOfEdges[14] = 7;
            collision.IndicesOfEdges[15] = 4;

            collision.IndicesOfEdges[16] = 0;
            collision.IndicesOfEdges[17] = 4;

            collision.IndicesOfEdges[18] = 1;
            collision.IndicesOfEdges[19] = 5;

            collision.IndicesOfEdges[20] = 2;
            collision.IndicesOfEdges[21] = 6;

            collision.IndicesOfEdges[22] = 3;
            collision.IndicesOfEdges[23] = 7;
            #endregion

            return collision;
        }

        /**
         * This intersection test by using vector directions which basically checks when the point moves from
         * its pervious position, if the new position it is in intersects with a plane.
         * */
        public static bool CheckPlaneIntersection(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            //we substract everything by b in order to make B the origin which allows for easier
            //calculation
            oldPoint -= B;
            newPoint -= B;
            A -= B;
            C -= B;

            //We then calculate the normal of the plane so that we can compare the old position with the new position
            //and decide whether an intersection has occured.
            Vector3 normal = Vector3.Cross(A, C);
            float buff1 = Vector3.Dot(oldPoint, normal);
            float buff2 = Vector3.Dot(newPoint, normal);
            return ((buff1 >= 0) && (buff2 <= 0));
        }

        /**
         * This method behaves exactly like the previous one but it takes into account the times when
         * a line is trying to be intersected in which case it will return false.
         * */
        public static bool CheckPlaneIntersection2(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            oldPoint -= B;
            newPoint -= B;
            A -= B;
            C -= B;
            Vector3 normal = Vector3.Cross(A*1000, C*1000);
            if (normal == Vector3.Zero)
            {
                return false;
            }
            else
            {
                float buff1 = Vector3.Dot(oldPoint, normal);
                float buff2 = Vector3.Dot(newPoint, normal);
                //now the vertices only have to be on the opposite side so we need
                //and additional “or”
                return (((buff1 >= 0) && (buff2 <= 0)) || ((buff1 <= 0) && (buff2 >= 0)));
            }
        }

        /**
         * This uses ray polygon intersection test to check if collision occurse based on the 
         * normals in which they have. It moves it into axes based on the different edges of the
         * triangle and then checks the orientation of the normals on each axis. If all the axis has the
         * same normal orientation, this means that it is intersecting.
         * 
         * */
        public static bool CheckRayTriangleIntersection(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 O, N, V, Cross;

            //check for BA
            V = B - A;
            O = oldPoint - A;
            N = newPoint - A;
            Cross = Vector3.Cross(V, O);
            if (Vector3.Dot(Cross, N) <= 0f)
                return false;
            else
            {
                //check for BC
                V = C - B;
                O = oldPoint - B;
                N = newPoint - B;
                Cross = Vector3.Cross(V, O);
                if (Vector3.Dot(Cross, N) <= 0f)
                    return false;
                else
                {
                    //check for AC
                    V = A - C;
                    O = oldPoint - C;
                    N = newPoint - C;
                    Cross = Vector3.Cross(V, O);
                    if (Vector3.Dot(Cross, N) <= 0f)
                        return false;
                    else
                        return true;
                }
            }

        }

        /**
         * This is the same basic test as the pervious method but it instead uses an edge square
         * intersection test. This means there is another extra point in the intersection test.
         * 
         * */
        public static bool CheckRayTriangleIntersection(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            Vector3 O, N, V, Cross;

            V = B - A;
            O = oldPoint - A;
            N = newPoint - A;
            Cross = Vector3.Cross(V, O);
            
            if (Vector3.Dot(Cross, N) < 0f)
            {
                V = C - B;
                O = oldPoint - B;
                N = newPoint - B;
                Cross = Vector3.Cross(V, O);
                if (Vector3.Dot(Cross, N) >= 0f)
                {
                    return false;
                }
                else
                {
                    V = D - C;
                    O = oldPoint - C;
                    N = newPoint - C;
                    Cross = Vector3.Cross(V, O);
                    if (Vector3.Dot(Cross, N) >= 0f)
                    {
                        return false;
                    }
                    else
                    {
                        V = A - D;
                        O = oldPoint - D;
                        N = newPoint - D;
                        Cross = Vector3.Cross(V, O);
                        if (Vector3.Dot(Cross, N) >= 0f)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                V = C - B;
                O = oldPoint - B;
                N = newPoint - B;
                Cross = Vector3.Cross(V, O);
                if (Vector3.Dot(Cross, N) <= 0f)
                {
                    return false;
                }
                else
                {
                    V = D - C;
                    O = oldPoint - C;
                    N = newPoint - C;
                    Cross = Vector3.Cross(V, O);
                    if (Vector3.Dot(Cross, N) <= 0f)
                    {
                        return false;
                    }
                    else
                    {
                        V = A - D;
                        O = oldPoint - D;
                        N = newPoint - D;
                        Cross = Vector3.Cross(V, O);
                        if (Vector3.Dot(Cross, N) <= 0f)
                        {
                            return false;
                        }
                        else
                            return true;
                    }
                }
            }
        }

        /**
         * This method calculates the distance in which it intersects so as to allow for the
         * movement to move back
         * */
        public static float Intersection(Vector3 LinePoint1, Vector3 dir, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 normal = Vector3.Cross(A-B,C-B);
            Vector3 org = LinePoint1 - B;
            float U = -Vector3.Dot(org, normal) / Vector3.Dot(dir, normal);
            return U;
        }

        /**
         * This method returns the world vertices of the current mesh
         * This is used in the intersection tests
         * */
        public Vector3[] ReturnWorldVertices()
        {
            Vector3[] worldVertices = new Vector3[Vertices.Length];
            for (byte i = 0; i < Vertices.Length; i++)
            {
                worldVertices[i] = Vector3.Transform(Vertices[i], WorldMatrix);
            }
            return worldVertices;
        }
        
        /**
         * This is similar to the other intersection test but this one returns the new movement
         * vector based on the collision checks of the edges.
         * */
        public Vector3 EdgeIntersection(Vector3 A, Vector3 B, Vector3 E, Vector3 movement)
        {
            float dirx = B.X - A.X;
            float diry = B.Y - A.Y;
            float t = ((A.Y - E.Y) * dirx + (E.X - A.X) * diry) / (movement.Y * dirx - movement.X * diry);
            Vector3 point = E + movement * t;
            point = E - point;
            return point;
        }

        /**
         * This is the move vector in which the movements are calculated and checked for every
         * one of the objects. If these two objects collide, a new move vector is the calculated.
         * This new move vector then ensures that the object does not go into the other object
         * and thus providing a more realistic feel for collision.
         * 
         * */
        public void Move(Vector3 movement, CollisionMesh otherMesh)
        {
            Vector3[] This = ReturnWorldVertices();
            Vector3[] Other = otherMesh.ReturnWorldVertices();
            Vector3[] New = new Vector3[This.Length];

            bool changed = true;
            int index = -1;
            for (int i = 0; i < This.Length; i++)
            {
                if (changed)
                {
                    if (index == i)
                    {
                        changed = false;
                    }
                    else
                    {
                        New[i] = This[i] + movement;
                    }
                }
                for (int j = 0; j < otherMesh.Indices.Length; j += 3)
                {
                    if ((CheckPlaneIntersection(This[i], New[i], Other[otherMesh.Indices[j]], Other[otherMesh.Indices[j + 1]], Other[otherMesh.Indices[j + 2]])) &&
                        (CheckRayTriangleIntersection(This[i], New[i], Other[otherMesh.Indices[j]], Other[otherMesh.Indices[j + 1]], Other[otherMesh.Indices[j + 2]])))
                    {
                        movement = movement * (Intersection(This[i], movement, Other[otherMesh.Indices[j]], Other[otherMesh.Indices[j + 1]], Other[otherMesh.Indices[j + 2]]) * FLOAT_ERROR);
                        changed = true;
                        New[i] = This[i] + movement;
                        index = i;
                    }
                }
            }

            Vector3[] OtherMoved = new Vector3[Other.Length];
            index = -1;
            changed = true;
            for (int i = 0; i < Other.Length; i++)
            {
                if (changed)
                {
                    if (index == i)
                    {
                        changed = false;
                    }
                    else
                    {
                        OtherMoved[i] = Other[i] - movement;
                    }

                }
                for (int j = 0; j < Indices.Length; j += 3)
                {
                    if ((CheckPlaneIntersection(Other[i], OtherMoved[i], This[Indices[j]], This[Indices[j + 1]], This[Indices[j + 2]])) &&
                        (CheckRayTriangleIntersection(Other[i], OtherMoved[i], This[Indices[j]], This[Indices[j + 1]], This[Indices[j + 2]])))
                    {
                        movement = movement * (Intersection(Other[i], -movement, This[Indices[j]], This[Indices[j + 1]], This[Indices[j + 2]]) * FLOAT_ERROR);
                        changed = true;
                        OtherMoved[i] = Other[i] - movement;
                        index = i;
                    }
                }
            }

            int l = 0;
            while ((changed) && (l < This.Length))
            {
                if (index == l)
                {
                    changed = false;
                }
                else
                {
                    New[l] = This[l] + movement;
                }
                l++;
            }

            for (int i = 0; i < otherMesh.IndicesOfEdges.Length; i+=2)
            {
                for (int j = 0; j < IndicesOfEdges.Length; j += 2)
                {
                    if ((CheckPlaneIntersection2(Other[otherMesh.IndicesOfEdges[i]], Other[otherMesh.IndicesOfEdges[i+1]], This[IndicesOfEdges[j]], New[IndicesOfEdges[j+1]], This[IndicesOfEdges[j+1]])) &&
                        (CheckRayTriangleIntersection(Other[otherMesh.IndicesOfEdges[i]], Other[otherMesh.IndicesOfEdges[i + 1]], This[IndicesOfEdges[j]], This[IndicesOfEdges[j + 1]], New[IndicesOfEdges[j + 1]], New[IndicesOfEdges[j]])))
                    {
                        Vector3 direction = Other[otherMesh.IndicesOfEdges[i+1]] - Other[otherMesh.IndicesOfEdges[i]];
                        float U = Intersection(Other[otherMesh.IndicesOfEdges[i]], direction, This[IndicesOfEdges[j]], New[IndicesOfEdges[j+1]], This[IndicesOfEdges[j+1]]);
                        Vector3 Point = Other[otherMesh.IndicesOfEdges[i]] + U * direction;
                        movement = EdgeIntersection(This[IndicesOfEdges[j]], This[IndicesOfEdges[j + 1]], Point, movement) * FLOAT_ERROR;

                        for (int k = 0; k < This.Length; k++)
                        {
                            New[k] = This[k] + movement;
                        }
                    }
                }
            }

            if (movement.Length() > 0.0001f)
            {
                Position += movement;
                isChanged = true;
                RefreshMatrix();
            }
        }
    }
}
