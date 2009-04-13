﻿using System;
using System.Collections.Generic;
using System.Text;
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
using Muffin.Components.Collision;

namespace Definitions
{
    public class TerrainObject : GameObject
    {
        private Matrix _worldMatrix;

        /*
         * This is the constructor of a terrain object.  It makes some assumptions about the object,
         * such as its mass, model type, and the fact that it is locked.
         * */

        public TerrainObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions, float scale, int x, int y) :
            base(model, ModelType.TERRAIN, modelName, position, rotation, true, dimensions, float.MaxValue, scale)
        {
            // the one thing we want to do is store the worldMatrix so that it does not have to be recalculated
            _worldMatrix = base.worldMatrix();
            gridX = x;
            gridY = y;
        }

        /*
         * This method overrides that of the base class by returning a precalculated value,
         * as terrain never moves.
         * */

        public override Matrix worldMatrix()
        {
            return _worldMatrix;
        }

        /*
         * Same as above--there is no need to compute this, as this object can't change position.
         * */

        public override Matrix futureWorldMatrix()
        {
            return _worldMatrix;
        }

        /*
         * Again, no need to recompute each time, as this game object never moves.
         * */

        public override OrientedBoundingBox getCurrentBoundingBox()
        {
            return _boundingBox;
        }

        // These represent the (x,y) location of this terrain object in the world so the AI can easily figure out
        // which tiles are adjacent to which
        public int gridX { get; set; }
        public int gridY { get; set; }
    }
}
