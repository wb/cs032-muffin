using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;
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
using Muffin.Components.Renderer;
using Muffin.Components.Physics;
using Muffin.Components.UI;
using Muffin.Components.AI;


namespace Muffin.Components.Collision
{
    public class OBB
    {
        [Flags()]
        enum DirtyFlags
        {
            None = 0,
            InverseRotationDirty = 1,
            WorldTransformDirty = 2,
            LocalAABBDirty = 4,
        }

        private DirtyFlags dirtyFlags;


        private Vector3 bounds;

        public Vector3 Bounds
        {
            get { return bounds; }
            set
            {
                bounds = value;
                dirtyFlags |= DirtyFlags.LocalAABBDirty;
            }
        }


        private Vector3 center;

        public Vector3 Center
        {
            get { return center; }
            set
            {
                center = value;
                dirtyFlags |= DirtyFlags.WorldTransformDirty;
            }
        }


        private Matrix rotation;

        public Matrix Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                dirtyFlags |= DirtyFlags.InverseRotationDirty;
                dirtyFlags |= DirtyFlags.WorldTransformDirty;
            }
        }

        Matrix inverseRotation;

        public Matrix InverseRotation
        {
            get
            {
                if ((dirtyFlags & DirtyFlags.InverseRotationDirty) ==
                        DirtyFlags.InverseRotationDirty)
                {
                    inverseRotation = Matrix.Invert(rotation);
                    dirtyFlags ^= DirtyFlags.InverseRotationDirty;
                }
                return inverseRotation;
            }
        }


        private Matrix worldTransform;

        public Matrix WorldTransform
        {
            get
            {
                if ((dirtyFlags & DirtyFlags.WorldTransformDirty) ==
                        DirtyFlags.WorldTransformDirty)
                {
                    worldTransform = Rotation * Matrix.CreateTranslation(center);
                    dirtyFlags ^= DirtyFlags.WorldTransformDirty;
                }
                return worldTransform;
            }
        }

        private BoundingBox aabb;
        public BoundingBox LocalBoundingBox
        {
            get
            {
                if ((dirtyFlags & DirtyFlags.LocalAABBDirty) ==
                        DirtyFlags.LocalAABBDirty)
                {
                    aabb.Max.X = bounds.X;
                    aabb.Max.Y = bounds.Y;
                    aabb.Max.Z = bounds.Z;

                    aabb.Min.X = -bounds.X;
                    aabb.Min.Y = -bounds.Y;
                    aabb.Min.Z = -bounds.Z;

                    dirtyFlags ^= DirtyFlags.LocalAABBDirty;
                }
                return aabb;
            }
        }



        public OBB()
            : this(Vector3.Zero, Vector3.One, Matrix.Identity)
        {
        }

        public OBB(Vector3 center, Vector3 bounds)
            : this(center, bounds, Matrix.Identity)
        {
        }


        public OBB(Vector3 center, Vector3 bounds, Matrix rotation)
        {
            this.center = center;
            this.bounds = bounds;

            this.rotation = rotation;

            this.inverseRotation = Matrix.Identity;

            worldTransform = Matrix.Identity;

            this.aabb = new BoundingBox();

            this.dirtyFlags = DirtyFlags.WorldTransformDirty |
                              DirtyFlags.InverseRotationDirty |
                              DirtyFlags.LocalAABBDirty;
        }



        public bool Intersects(OBB b)
        {
            Matrix matB = b.rotation * this.InverseRotation;
            Vector3 vPosB = Vector3.Transform(b.center - this.center,
                                              this.inverseRotation);

            Vector3 XAxis = new Vector3(matB.M11, matB.M21, matB.M31);
            Vector3 YAxis = new Vector3(matB.M12, matB.M22, matB.M32);
            Vector3 ZAxis = new Vector3(matB.M13, matB.M23, matB.M33);

            //15 tests

            //1 (Ra)x
            if ((float)Math.Abs(vPosB.X) >
                    (this.bounds.X +
                        b.bounds.X * (float)Math.Abs(XAxis.X) +
                        b.bounds.Y * (float)Math.Abs(XAxis.Y) +
                        b.bounds.Z * (float)Math.Abs(XAxis.Z)))
            {
                return false;
            }

            //2 (Ra)y
            if ((float)Math.Abs(vPosB.Y) >
                    (this.bounds.Y +
                        b.bounds.X * (float)Math.Abs(YAxis.X) +
                        b.bounds.Y * (float)Math.Abs(YAxis.Y) +
                        b.bounds.Z * (float)Math.Abs(YAxis.Z)))
            {
                return false;
            }

            //3 (Ra)z
            if ((float)Math.Abs(vPosB.Z) >
                    (this.bounds.Z +
                        b.bounds.X * (float)Math.Abs(ZAxis.X) +
                        b.bounds.Y * (float)Math.Abs(ZAxis.Y) +
                        b.bounds.Z * (float)Math.Abs(ZAxis.Z)))
            {
                return false;
            }

            //4 (Rb)x
            if ((float)Math.Abs(vPosB.X * XAxis.X +
                                vPosB.Y * YAxis.X +
                                vPosB.Z * ZAxis.X) >
                    (b.bounds.X +
                        this.bounds.X * (float)Math.Abs(XAxis.X) +
                        this.bounds.Y * (float)Math.Abs(YAxis.X) +
                        this.bounds.Z * (float)Math.Abs(ZAxis.X)))
            {
                return false;
            }

            //5 (Rb)y
            if ((float)Math.Abs(vPosB.X * XAxis.Y +
                                vPosB.Y * YAxis.Y +
                                vPosB.Z * ZAxis.Y) >
                    (b.bounds.Y +
                        this.bounds.X * (float)Math.Abs(XAxis.Y) +
                        this.bounds.Y * (float)Math.Abs(YAxis.Y) +
                        this.bounds.Z * (float)Math.Abs(ZAxis.Y)))
            {
                return false;
            }

            //6 (Rb)z
            if ((float)Math.Abs(vPosB.X * XAxis.Z +
                                vPosB.Y * YAxis.Z +
                                vPosB.Z * ZAxis.Z) >
                    (b.bounds.Z +
                        this.bounds.X * (float)Math.Abs(XAxis.Z) +
                        this.bounds.Y * (float)Math.Abs(YAxis.Z) +
                        this.bounds.Z * (float)Math.Abs(ZAxis.Z)))
            {
                return false;
            }

            //7 (Ra)x X (Rb)x
            if ((float)Math.Abs(vPosB.Z * YAxis.X -
                                vPosB.Y * ZAxis.X) >
                    (this.bounds.Y * (float)Math.Abs(ZAxis.X) +
                    this.bounds.Z * (float)Math.Abs(YAxis.X) +
                    b.bounds.Y * (float)Math.Abs(XAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(XAxis.Y)))
            {
                return false;
            }

            //8 (Ra)x X (Rb)y
            if ((float)Math.Abs(vPosB.Z * YAxis.Y -
                                vPosB.Y * ZAxis.Y) >
                    (this.bounds.Y * (float)Math.Abs(ZAxis.Y) +
                    this.bounds.Z * (float)Math.Abs(YAxis.Y) +
                    b.bounds.X * (float)Math.Abs(XAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(XAxis.X)))
            {
                return false;
            }

            //9 (Ra)x X (Rb)z
            if ((float)Math.Abs(vPosB.Z * YAxis.Z -
                                vPosB.Y * ZAxis.Z) >
                    (this.bounds.Y * (float)Math.Abs(ZAxis.Z) +
                    this.bounds.Z * (float)Math.Abs(YAxis.Z) +
                    b.bounds.X * (float)Math.Abs(XAxis.Y) +
                    b.bounds.Y * (float)Math.Abs(XAxis.X)))
            {
                return false;
            }

            //10 (Ra)y X (Rb)x
            if ((float)Math.Abs(vPosB.X * ZAxis.X -
                                vPosB.Z * XAxis.X) >
                    (this.bounds.X * (float)Math.Abs(ZAxis.X) +
                    this.bounds.Z * (float)Math.Abs(XAxis.X) +
                    b.bounds.Y * (float)Math.Abs(YAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(YAxis.Y)))
            {
                return false;
            }

            //11 (Ra)y X (Rb)y
            if ((float)Math.Abs(vPosB.X * ZAxis.Y -
                                vPosB.Z * XAxis.Y) >
                    (this.bounds.X * (float)Math.Abs(ZAxis.Y) +
                    this.bounds.Z * (float)Math.Abs(XAxis.Y) +
                    b.bounds.X * (float)Math.Abs(YAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(YAxis.X)))
            {
                return false;
            }

            //12 (Ra)y X (Rb)z
            if ((float)Math.Abs(vPosB.X * ZAxis.Z -
                                vPosB.Z * XAxis.Z) >
                    (this.bounds.X * (float)Math.Abs(ZAxis.Z) +
                    this.bounds.Z * (float)Math.Abs(XAxis.Z) +
                    b.bounds.X * (float)Math.Abs(YAxis.Y) +
                    b.bounds.Y * (float)Math.Abs(YAxis.X)))
            {
                return false;
            }

            //13 (Ra)z X (Rb)x
            if ((float)Math.Abs(vPosB.Y * XAxis.X -
                                vPosB.X * YAxis.X) >
                    (this.bounds.X * (float)Math.Abs(YAxis.X) +
                    this.bounds.Y * (float)Math.Abs(XAxis.X) +
                    b.bounds.Y * (float)Math.Abs(ZAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(ZAxis.Y)))
            {
                return false;
            }

            //14 (Ra)z X (Rb)y
            if ((float)Math.Abs(vPosB.Y * XAxis.Y -
                                vPosB.X * YAxis.Y) >
                    (this.bounds.X * (float)Math.Abs(YAxis.Y) +
                    this.bounds.Y * (float)Math.Abs(XAxis.Y) +
                    b.bounds.X * (float)Math.Abs(ZAxis.Z) +
                    b.bounds.Z * (float)Math.Abs(ZAxis.X)))
            {
                return false;
            }

            //15 (Ra)z X (Rb)z
            if ((float)Math.Abs(vPosB.Y * XAxis.Z -
                                vPosB.X * YAxis.Z) >
                    (this.bounds.X * (float)Math.Abs(YAxis.Z) +
                    this.bounds.Y * (float)Math.Abs(XAxis.Z) +
                    b.bounds.X * (float)Math.Abs(ZAxis.Y) +
                    b.bounds.Y * (float)Math.Abs(ZAxis.X)))
            {
                return false;
            }

            return true;
        }
    }
}
