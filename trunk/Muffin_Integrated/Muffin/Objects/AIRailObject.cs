using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Muffin.Components.AI;

namespace Definitions
{
    class AIRailObject : AIObject
    {
        private bool _loop;
        private int _node;
        private int _pathDir;

        public AIRailObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions, float mass, float scale, bool loop) :
            base(model, modelName, position, rotation, dimensions, mass, scale)
        {
            _loop = loop;
            _node = 0;
            _pathDir = 1;
        }

        public override void controlInput(Vector2 dir, bool jump)
        {
            _toMove = new Vector3(dir.X, 0, dir.Y);

            //_orientation = Quaternion.Slerp(_orientation, Quaternion.CreateFromAxisAngle(Vector3.Up, angle),0.01f);
            if (jump && _jumpCount < 2)
            {
                _jumpCount++;

                // clear any previous force in the y direction
                _force.Y = 0;

                // set acceleration to 0 in the y direction
                currentState.acceleration = new Vector3(currentState.acceleration.X, 0, currentState.acceleration.Z);
                futureState.acceleration = new Vector3(futureState.acceleration.X, 0, futureState.acceleration.Z);

                // set velocity to a given amount
                float amount = 200;
                currentState.velocity = new Vector3(currentState.velocity.X, amount, currentState.velocity.Z);
                futureState.velocity = new Vector3(futureState.velocity.X, amount, currentState.velocity.Z);

                this.applyForce(new Vector3(0.0f, 1000.0f * _mass, 0.0f), _dimensions / 2.0f);

            }

        }

        protected override void setDirection()
        {
            if (_path != null && _path.Count > 0)
            {
                Vector3 curDest = _path[_node];
                Vector3 direction = curDest - position;
                direction.Y = 0;
                if (direction.Length() < 1)
                {
                    int next = _node + _pathDir;
                    if (next < 0 || next >= _path.Count)
                    {
                        if (_loop)
                        {
                            _pathDir = -_pathDir;
                            next = _node + _pathDir;
                        }
                        else
                            next = 0;
                    }
                    if (next >= 0 && next < _path.Count)
                    {
                        _node = next;
                        curDest = _path[next];
                        direction = curDest - position;
                        direction.Y = 0;
                    }
                }

                direction.Normalize();
                controlInput(new Vector2(direction.X, direction.Z), false);
            }
        }

        public override void doAI(AI a)
        {
            setDirection();
        }


        public override void applyForce(Vector3 force, Vector3 location)
        {
            // DO NOTHING!
        }

     

        public override void applyForceAtCenter(Vector3 force)
        {
            // DO NOTHING!
        }

    }
}
