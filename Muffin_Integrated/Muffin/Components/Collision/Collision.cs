using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace ModelCollision
{
    public class CollisionMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public int[] IndicesOfEdges;
        public Vector3 Position;
        public Vector3 Rotation;
        public Matrix WorldMatrix;
        public bool isChanged;
        public BoundingBox boundingBox;
        static readonly float TOWORK = 0.99f;

        public Vector3[] updatedVertices;

        public CollisionMesh(Vector3 position, Vector3 rotatio)
        {
            Position = position;
            Rotation = rotatio;
            isChanged = true;
        }

        public void RefreshMatrix()
        {
            if (isChanged)
            {
                WorldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateTranslation(Position);
                UpdateVertices();
                isChanged = false;
            }
        }

        public static CollisionMesh FromModel(Model model, Vector3 position, Vector3 rotation)
        {
            CollisionMesh collision = new CollisionMesh(position, rotation);

            Dictionary<string, object> tagData = model.Tag as Dictionary<string, object>;
            Vector3[] vertices = tagData["Vertices"] as Vector3[];
            int[] indices = tagData["Indices"] as int[];

            collision.Vertices = vertices;
            collision.Indices = indices;
            collision.IndicesOfEdges = GetIndicesOfEdges(indices);

            collision.boundingBox = new BoundingBox();
            collision.updatedVertices = new Vector3[vertices.Length];
            collision.RefreshMatrix();
            return collision;
        }

        #region CollisionMath
        public static bool CheckPlaneIntersection(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            oldPoint -= B;
            newPoint -= B;
            A -= B;
            C -= B;
            Vector3 normal = Vector3.Cross(A, C);
            float buff1 = Vector3.Dot(oldPoint, normal);
            float buff2 = Vector3.Dot(newPoint, normal);
            return ((buff1 >= 0) && (buff2 <= 0));
        }

        public static bool CheckPlaneIntersection2(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            oldPoint -= B;
            newPoint -= B;
            A -= B;
            C -= B;
            Vector3 normal = Vector3.Cross(A * 1000, C * 1000);
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

        public static bool CheckRayTriangleIntersection(Vector3 oldPoint, Vector3 newPoint, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 O, N, V, Cross;

            V = B - A;
            O = oldPoint - A;
            N = newPoint - A;
            Cross = Vector3.Cross(V, O);
            if (Vector3.Dot(Cross, N) <= 0f)
                return false;
            else
            {
                V = C - B;
                O = oldPoint - B;
                N = newPoint - B;
                Cross = Vector3.Cross(V, O);
                if (Vector3.Dot(Cross, N) <= 0f)
                    return false;
                else
                {
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

        public static float Intersection(Vector3 LinePoint1, Vector3 dir, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 normal = Vector3.Cross(A - B, C - B);
            Vector3 org = LinePoint1 - B;
            float U = -Vector3.Dot(org, normal) / Vector3.Dot(dir, normal);
            return U;
        }

        static int[] GetIndicesOfEdges(int[] indices)
        {
            List<int> indicesofedges = new List<int>();
            bool first, second, third;
            for (int i = 0; i < indices.Length; i += 3)
            {
                first = true;
                second = true;
                third = true;
                //check if we have added it before
                for (int j = 0; j < indicesofedges.Count; j += 2)
                {
                    if (((indicesofedges[j] == indices[i]) && (indicesofedges[j + 1] == indices[i + 1]))
                        || ((indicesofedges[j] == indices[i + 1]) && (indicesofedges[j + 1] == indices[i])))
                    {
                        first = false;
                    }
                    if (((indicesofedges[j] == indices[i + 1]) && (indicesofedges[j + 1] == indices[i + 2]))
                        || ((indicesofedges[j] == indices[i + 2]) && (indicesofedges[j + 1] == indices[i + 1])))
                    {
                        second = false;
                    }
                    if (((indicesofedges[j] == indices[i + 2]) && (indicesofedges[j + 1] == indices[i]))
                        || ((indicesofedges[j] == indices[i]) && (indicesofedges[j + 1] == indices[i + 2])))
                    {
                        third = false;
                    }
                }
                if (first)
                {
                    indicesofedges.Add(indices[i]);
                    indicesofedges.Add(indices[i + 1]);
                }
                if (second)
                {
                    indicesofedges.Add(indices[i + 1]);
                    indicesofedges.Add(indices[i + 2]);
                }
                if (third)
                {
                    indicesofedges.Add(indices[i + 2]);
                    indicesofedges.Add(indices[i]);
                }
            }
            return indicesofedges.ToArray();
        }

        public static CollisionMesh BoxPrimitive(float a, float b, float c, Vector3 position, Vector3 rotation)
        {
            CollisionMesh collision = new CollisionMesh(position, rotation);

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

            #region Indices
            collision.Indices = new int[36];

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

            #region IndicesOfEdges
            collision.IndicesOfEdges = new int[24];
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

        public Vector3 EdgeIntersection(Vector3 A, Vector3 B, Vector3 E, Vector3 movement)
        {
            float dirx = B.X - A.X;
            float diry = B.Y - A.Y;
            float t = ((A.Y - E.Y) * dirx + (E.X - A.X) * diry) / (movement.Y * dirx - movement.X * diry);
            Vector3 point = E + movement * t;
            point = E - point;
            return point;
        }

        #endregion

        public void UpdateVertices()
        {
            Matrix realMatrix = WorldMatrix;
            WorldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(180)) * WorldMatrix;
            for (int i = 0; i < updatedVertices.Length; i++)
            {
                updatedVertices[i] = Vector3.Transform(Vertices[i], WorldMatrix);
            }
            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            for (int i = 0; i < updatedVertices.Length; i++)
            {
                if (updatedVertices[i].X < Min.X) Min.X = updatedVertices[i].X;
                if (updatedVertices[i].X > Max.X) Max.X = updatedVertices[i].X;
                if (updatedVertices[i].Y < Min.Y) Min.Y = updatedVertices[i].Y;
                if (updatedVertices[i].Y > Max.Y) Max.Y = updatedVertices[i].Y;
                if (updatedVertices[i].Z < Min.Z) Min.Z = updatedVertices[i].Z;
                if (updatedVertices[i].Z > Max.Z) Max.Z = updatedVertices[i].Z;
            }
            boundingBox.Min = Min;
            boundingBox.Max = Max;
            WorldMatrix = realMatrix;
        }



        public bool Move(Vector3 movement, CollisionMesh otherMesh)
        {
            if (!boundingBox.Intersects(otherMesh.boundingBox))
            {
                Position += movement;
                isChanged = true;
                RefreshMatrix();
                return false;
            }

            Vector3[] New = new Vector3[updatedVertices.Length];

            bool changed = true;
            int index = -1;
            for (int i = 0; i < updatedVertices.Length; i++)
            {
                if (changed)
                {
                    if (index == i)
                    {
                        changed = false;
                    }
                    else
                    {
                        New[i] = updatedVertices[i] + movement;
                    }
                }
                for (int j = 0; j < otherMesh.Indices.Length; j += 3)
                {

                    if ((CheckPlaneIntersection(updatedVertices[i], New[i], otherMesh.updatedVertices[otherMesh.Indices[j]], otherMesh.updatedVertices[otherMesh.Indices[j + 1]], otherMesh.updatedVertices[otherMesh.Indices[j + 2]])) &&
                        (CheckRayTriangleIntersection(updatedVertices[i], New[i], otherMesh.updatedVertices[otherMesh.Indices[j]], otherMesh.updatedVertices[otherMesh.Indices[j + 1]], otherMesh.updatedVertices[otherMesh.Indices[j + 2]])))
                    {
                        movement = movement * (Intersection(updatedVertices[i], movement, otherMesh.updatedVertices[otherMesh.Indices[j]], otherMesh.updatedVertices[otherMesh.Indices[j + 1]], otherMesh.updatedVertices[otherMesh.Indices[j + 2]]) * TOWORK);
                        changed = true;
                        New[i] = updatedVertices[i] + movement;
                        index = i;
                    }
                }
            }

            Vector3[] OtherMoved = new Vector3[otherMesh.updatedVertices.Length];
            index = -1;
            changed = true;
            for (int i = 0; i < otherMesh.updatedVertices.Length; i++)
            {
                if (changed)
                {
                    if (index == i)
                    {
                        changed = false;
                    }
                    else
                    {
                        OtherMoved[i] = otherMesh.updatedVertices[i] - movement;
                    }

                }
                for (int j = 0; j < Indices.Length; j += 3)
                {
                    if ((CheckPlaneIntersection(otherMesh.updatedVertices[i], OtherMoved[i], updatedVertices[Indices[j]], updatedVertices[Indices[j + 1]], updatedVertices[Indices[j + 2]])) &&
                        (CheckRayTriangleIntersection(otherMesh.updatedVertices[i], OtherMoved[i], updatedVertices[Indices[j]], updatedVertices[Indices[j + 1]], updatedVertices[Indices[j + 2]])))
                    {
                        movement = movement * (Intersection(otherMesh.updatedVertices[i], -movement, updatedVertices[Indices[j]], updatedVertices[Indices[j + 1]], updatedVertices[Indices[j + 2]]) * TOWORK);
                        changed = true;
                        OtherMoved[i] = otherMesh.updatedVertices[i] - movement;
                        index = i;
                    }
                }
            }

            int l = 0;
            while ((changed) && (l < updatedVertices.Length))
            {
                if (index == l)
                {
                    changed = false;
                }
                else
                {
                    New[l] = updatedVertices[l] + movement;
                }
                l++;
            }

            for (int i = 0; i < otherMesh.IndicesOfEdges.Length; i += 2)
            {
                for (int j = 0; j < IndicesOfEdges.Length; j += 2)
                {
                    if ((CheckPlaneIntersection2(otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i]], otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i + 1]], updatedVertices[IndicesOfEdges[j]], New[IndicesOfEdges[j + 1]], updatedVertices[IndicesOfEdges[j + 1]])) &&
                        (CheckRayTriangleIntersection(otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i]], otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i + 1]], updatedVertices[IndicesOfEdges[j]], updatedVertices[IndicesOfEdges[j + 1]], New[IndicesOfEdges[j + 1]], New[IndicesOfEdges[j]])))
                    {
                        Vector3 direction = otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i + 1]] - otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i]];
                        float U = Intersection(otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i]], direction, updatedVertices[IndicesOfEdges[j]], New[IndicesOfEdges[j + 1]], updatedVertices[IndicesOfEdges[j + 1]]);
                        Vector3 Point = otherMesh.updatedVertices[otherMesh.IndicesOfEdges[i]] + U * direction;
                        movement = EdgeIntersection(updatedVertices[IndicesOfEdges[j]], updatedVertices[IndicesOfEdges[j + 1]], Point, movement) * TOWORK;

                        for (int k = 0; k < updatedVertices.Length; k++)
                        {
                            New[k] = updatedVertices[k] + movement;
                        }
                    }
                }
            }

            if (movement.Length() > 0.0001f)
            {
                Position += movement;
                isChanged = true;
                RefreshMatrix();
                return false;
            }
            else
            {
                //collision detected
                //System.Windows.Forms.MessageBox.Show("Collision detected");
                return true;
            }
        }
    }
}
