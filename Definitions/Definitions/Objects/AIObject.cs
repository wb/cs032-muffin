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
    class AIObject : GameObject
    {
        private int _health, _lives;
        /*
         * This constructor makes a few assumptions.  Namely that an AIObject is an enemy.
         * */

        public AIObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions, float mass, float scale) :
            base(model, ModelType.ENEMY, modelName, position, rotation, false, dimensions, mass, scale)
        {
            // constructor contents here
        }

        #region Gets and Sets

        public int health
        {
            get { return _health; }
            set { _health = value; }
        }

        public int lives
        {
            get { return _lives; }
            set { _lives = value; }
        }

        #endregion
    }
}
