using System;
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


namespace Definitions
{
    class TerrainObject : GameObject
    {
        private Matrix _worldMatrix;

        /*
         * This is the constructor of a terrain object.  It makes some assumptions about the object,
         * such as its mass, model type, and the fact that it is locked.
         * */

        public TerrainObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions) :
            base(model, ModelType.TERRAIN, modelName, position, rotation, true, dimensions, float.MaxValue)
        {
            // the one thing we want to do is store the worldMatrix so that it does not have to be recalculated
            _worldMatrix = base.worldMatrix();
        }

        /*
         * This method overrides that of the base class by returning a precalculated value,
         * as terrain never moves.
         * */

        new public Matrix worldMatrix()
        {
            return _worldMatrix;
        }
    }
}
