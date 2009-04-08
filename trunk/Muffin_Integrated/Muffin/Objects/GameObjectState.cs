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
    /**
     * This class is used to hold the game state of a GameObject, for the purpose of computing
     * future timesteps for collision resolution.
     * */

    public class GameObjectState
    {
        Vector3 _position, _velocity, _acceleration, _angularVelocity, _angularAcceleration;
        Quaternion _rotation;

        public GameObjectState(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
            _velocity = new Vector3();
            _acceleration = new Vector3();
            _angularVelocity = new Vector3();
            _angularAcceleration = new Vector3();
        }

        /**
         * This method is used to copy another game object state to itself.
         * */

        public void copy(GameObjectState state)
        {
            _position = state.position;
            _velocity = state.velocity;
            _acceleration = state.acceleration;
            _rotation = state.rotation;
            _angularVelocity = state.angularVelocity;
            _angularAcceleration = state._angularAcceleration;
        }

        #region Gets and Sets

        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public Vector3 acceleration
        {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public Quaternion rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Vector3 angularVelocity
        {
            get { return _angularVelocity; }
            set { _angularVelocity = value; }
        }

        public Vector3 angularAcceleration
        {
            get { return _angularAcceleration; }
            set { _angularAcceleration = value; }
        }

        #endregion
    }
}
