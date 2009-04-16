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
using Muffin.Components.AI;

namespace Definitions
{
    public enum AIState {Idle, Active};

    public class AIObject : GameObject
    {
        private int _health, _lives;
        private AIState _state;

        private List<Vector3> _path;

        /*
         * This constructor makes a few assumptions.  Namely that an AIObject is an enemy.
         * */

        public AIObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions, float mass, float scale) :
            base(model, ModelType.ENEMY, modelName, position, rotation, false, dimensions, mass, scale)
        {
            _state = AIState.Idle;
            _path = new List<Vector3>();
        }

        public override void controlInput(Vector2 dir, bool jump)
        {
            _toMove = new Vector3(dir.X, 0, dir.Y);

            // for effect, update the orientation of the object to reflect where it is moving
            float angle = -(float)Math.Atan(dir.Y / dir.X);
            if (float.IsNaN(angle))
                angle = 0;

           
            // this should work, as long as AI is not being tracked by the camera
            _orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle);
            //_orientation = Quaternion.Slerp(_orientation, Quaternion.CreateFromAxisAngle(Vector3.Up, angle),0.01f);
            if (jump)
            {
                this.applyForce(new Vector3(0.0f, 1500.0f * _mass, 0.0f), _dimensions / 2.0f);
            }

        }

        public override Matrix worldMatrix()
        {
            return Matrix.CreateFromQuaternion(_orientation) *
                   Matrix.CreateTranslation(_currentState.position) *
                   Matrix.CreateScale(_scale);
        }

        private void setDirection()
        {
            if (_path.Count > 0)
            {
                Vector3 curDest = _path[0];
                Vector3 direction = curDest - position;
                direction.Y = 0;
                if (direction.Length() < 1)
                {
                    _path.RemoveAt(0);
                }
                else
                {
                    direction.Normalize();
                    controlInput(new Vector2(direction.X, direction.Z), false);
                }
            }
        }

        public void doAI(AI a)
        {
            // Dumb AI, tries to reach the player

            _path.Clear();
            _path.Add(a.game.allPlayer[0].position);

            setDirection();
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

        public List<Vector3> dest { get { return _path; } }

        #endregion
    }
}
