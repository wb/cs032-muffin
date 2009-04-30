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
    public class PlayerObject : GameObject
    {
        private int _health, _lives, _coinCount;
        
        /*
         * This constructor makes a few assumptions about a PlayerObject - namely that it is of ModelType human.
         * */

        public PlayerObject(Model model, ModelName modelName, Vector3 position, Quaternion rotation, Vector3 dimensions, float mass, float scale) :
            base(model, ModelType.HUMAN, modelName, position, rotation, false, dimensions, mass, scale)
        {
            // they aren't jumping! duh
            _jumpCount = 0;

            // no coins
            _coinCount = 0;
        }

        public override void controlInput(Vector2 dir, bool jump)
        {
            _toMove = new Vector3(dir.X, 0, dir.Y);

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

                this.applyForce(new Vector3(0.0f, 500.0f * _mass, 0.0f), _dimensions / 2.0f);

            }

        }

        /*
         * This method takes input from the keyboard or controller and moves the model accordingly.
         * upDown is used for moving backwards and forwards. - this should be normalized to a max absolute value of 1
         * leftRight is used for changing orientation or strafing (is strafe is true).  - this should be normalized to a max absolute value of 1
         * strafeValue is used for straffing  - this should be normalized to a max absolute value of 1
         * strafe toggles strafe mode (if using the keyboard, simply return true if either of the strafe keys are pressed).
         * jump toggles jump
         * 
         * An example usage of this is as follows (g is a GamePadState):
         * 
         * _gameObject.move(g.ThumbSticks.Left.Y, g.ThumbSticks.Left.X, g.ThumbSticks.Left.X, (g.Buttons.A == ButtonState.Pressed), (g.Buttons.X == ButtonState.Pressed));
         * 
         * */

        public override void move(float upDown, float leftRight, float strafeValue, Boolean jump, Boolean strafe)
        {
            Vector3 directionVector;
            Vector3 strafeVector;

            // strafe
            if (strafe)
            {
                float strafeState = (strafeValue == 0 ? 0 : 1);
                strafeVector = Vector3.UnitZ;
                float strafeAngle = (float)(Math.PI) / 2.0f * strafeState;


                strafeVector = -strafeValue * 2.0f * Vector3.Transform(strafeVector, Matrix.CreateFromQuaternion(_orientation) * Matrix.CreateFromAxisAngle(Vector3.Up, strafeAngle));

                directionVector = Vector3.Transform(new Vector3(0.0f, 0.0f, upDown * 2.0f), Matrix.CreateFromQuaternion(_orientation));

            }
            // otherwise just move
            else
            {
                float yaw = leftRight * MathHelper.ToRadians(-1.7f);
                Quaternion rot = Quaternion.CreateFromAxisAngle(Vector3.Up, yaw);
                _orientation *= rot;
                directionVector = Vector3.Transform(new Vector3(0.0f, 0.0f, upDown * 2.0f), Matrix.CreateFromQuaternion(_orientation));

                strafeVector = Vector3.Zero;
            }

            //direction = direction + strafe;
            this.controlInput(new Vector2(directionVector.X + strafeVector.X, directionVector.Z + strafeVector.Z), jump);

        }

        public override Matrix worldMatrix()
        {
            return Matrix.CreateFromQuaternion(_orientation) *
                   Matrix.CreateScale(_scale) *
                   Matrix.CreateTranslation(_futureState.position * _scale);
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

        // call this when a coin is collected

        public void coinCollected()
        {
            _coinCount++;
           
        }

        // call this to return coin count
        public int coinCount
        {
            get { return _coinCount; }
        }

        #endregion
    }
}
