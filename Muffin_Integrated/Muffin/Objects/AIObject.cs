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
    public class AIObject : GameObject
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

        public override void controlInput(Vector2 dir, bool jump)
        {
            _toMove = new Vector3(dir.X, 0, dir.Y);

            // for effect, update the orientation of the object to reflect where it is moving
            float angle = (float)Math.Atan(dir.Y / dir.X);
            if (float.IsNaN(angle))
                angle = 0;

            // this should work, as long as AI is not being tracked by the camera
            //_orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle);

            if (jump)
            {
                this.applyForce(new Vector3(0.0f, 1500.0f * _mass, 0.0f), _dimensions / 2.0f);
            }

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
