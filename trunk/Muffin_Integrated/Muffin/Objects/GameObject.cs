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
    public class GameObject
    {
        /*
         * Please keep all of these private! If you need to access
         * one of them that doesn't have a get/set, please add a
         * get/set in the appropriate place at the end of this file.
         * */

        private Model _model;
        private ModelType _modelType;
        private Material _material;
        private float _mass, _scale;
        private ModelName _modelName;
        private Vector3 _force, _centerOfMass, _torque, _dimensions;
        private Matrix _intertiaTensor;
        private Boolean _locked, _active;
        private BoundingBox _boundingBox;
        private GameObjectState _currentState, _futureState;

        /*
         * This is the constructor.  
         * */

        public GameObject(Model model, ModelType modelType, ModelName modelName, Vector3 position, Quaternion rotation, Boolean locked, Vector3 dimensions, float mass, float scale)
        {
            // initialize the game state objects (the future one should be the current one for now)
            _currentState = new GameObjectState(position, rotation);
            _futureState = new GameObjectState(position, rotation);

            // set the values that have been passed in
            _model = model;
            _modelType = modelType;
            _modelName = modelName;
            _locked = locked;
            _dimensions = dimensions;
            _mass = mass;
            _scale = scale;

            //initialize rest of the parameters to their defaults
            _force = new Vector3();

            // calculations for center of mass
            _centerOfMass = new Vector3(dimensions.X / 2, dimensions.Y / 2, dimensions.Z / 2);

            // TODO: UPDATE THIS -- THIS ASSUMES A CUBE-LIKE SHAPE
            _intertiaTensor = new Matrix(1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 0);

            // calculate the bounding box
            this.updateBoundingBox();

            _active = true;
        }

        /*
         * This method is used to set the input to a movable object
         * It is up to the physics engine to calculate a force vector that resolves this input
         * 
         * Since it is virtual, descendant classes can override this to handle it however they need to
         * And something immobile (like terrain), can just leave it empty so input is ignored if any happens
         * to get passed to it
         * */

        public virtual void controlInput(Vector2 dir, bool jump)
        {
        }

        /*
         * This method is used to update the bounding box whenever the object has moved.
         * */

        public void updateBoundingBox()
        {
            Vector3 min = _futureState.position;
            Vector3 max = _futureState.position + _dimensions;
            _boundingBox = new BoundingBox(min, max);
        }

        /*
         * This method returns the world matrix.  For any object
         * that extends game object, such as TerrainObject,
         * worldMatrix will be stored so that it does not
         * have to be recalculated each and every timestep.
         * */

        public Matrix worldMatrix()
        {
            return Matrix.CreateScale(_scale) *
                   Matrix.CreateFromQuaternion(_currentState.rotation) *
                   Matrix.CreateTranslation(_currentState.position);
        }

        /*
         * This method is used to apply force to the object at
         * a given location.
         * */

        public void applyForce(Vector3 force, Vector3 location)
        {
            // add this force on to the linear force
            _force += force;

            // the location is the location, so to find torque, find the cross product of (location - centerOfMass) and force
            location = location - _centerOfMass;

            Vector3.Cross(location, force);
            _torque += location;

        }

        public void integrate(float timestep)
        {
            prePhysics();

            // do the integration only if this object is not locked and is currently active
            if (!_locked && _active)
            {
                // first, solve for the new rotational position (orientation)
                Vector3 temp = new Vector3();

                temp = Vector3.Transform(_torque, _intertiaTensor);

                _futureState.angularAcceleration += temp;

                _futureState.angularVelocity = _futureState.angularVelocity + _futureState.angularAcceleration * timestep;

                Quaternion deltaOrientation = Quaternion.CreateFromAxisAngle(Vector3.Right, _futureState.angularVelocity.X * timestep) * Quaternion.CreateFromAxisAngle(Vector3.Up, _futureState.angularVelocity.Y * timestep) * Quaternion.CreateFromAxisAngle(Vector3.Backward, _futureState.angularVelocity.Z * timestep);

                deltaOrientation.Normalize();

                _futureState.rotation *= deltaOrientation;

                // now, solve for the new position
                _futureState.acceleration += _force / _mass;
                _futureState.velocity = _futureState.velocity + _futureState.acceleration * timestep;

                _futureState.position = _futureState.position + _futureState.velocity * timestep;

                // account for air resistance, general drag, etc
                _futureState.velocity = 0.995f * _futureState.velocity;
                _futureState.angularVelocity = 0.995f * _futureState.angularVelocity;

                this.updateBoundingBox();

            }
        }

        /*
         * This method is used before physics runs to initialize anything
         * before the actual calculations take place.
         * */

        public void prePhysics()
        {
            // intentionally left blank for now!
        }

        /*
         * This method is called everytime that physics finishes an
         * iteration. It is used to clean up, set forces/torques
         * to zero, etc.
         * */

        public void postPhysics(Boolean move)
        {
            // reset the force and torque to zero
            _force = Vector3.Zero;
            _torque = Vector3.Zero;

            if (move)
                _currentState.copy(_futureState);
            else
                _futureState.copy(_currentState);

        }


        #region Gets and Sets

        /*
         * Below are all of the gets and sets.  Please use gets and setes
         * instead of making variables public if you need to access them.
         * Follow the naming scheme below by using the variable name without
         * the preceeding underscore.
         * */

        public BoundingBox boundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        public ModelName modelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }

        public ModelType modelType
        {
            get { return _modelType; }
            set { _modelType = value; }
        }

        public Model model
        {
            get { return _model; }
            set { _model = value; }
        }

        public float mass
        {
            get { return _mass; }
            set { _mass = value; }
        }

        public Vector3 position
        {
            get { return _currentState.position; }
            set { _currentState.position = value; }
        }

        public Vector3 velocity
        {
            get { return _currentState.velocity; }
            set { _currentState.velocity = value; }
        }

        public Vector3 acceleration
        {
            get { return _currentState.acceleration; }
            set { _currentState.acceleration = value; }
        }

        public Vector3 force
        {
            get { return _force; }
            set { _force = value; }
        }

        public Vector3 centerOfMass
        {
            get { return _centerOfMass; }
            set { _centerOfMass = value; }
        }

        public Quaternion rotation
        {
            get { return _currentState.rotation; }
            set { _currentState.rotation = value; }
        }

        public Vector3 angularVelocity
        {
            get { return _currentState.angularVelocity; }
            set { _currentState.angularVelocity = value; }
        }

        public Vector3 angularAcceleration
        {
            get { return _currentState.angularAcceleration; }
            set { _currentState.angularAcceleration = value; }
        }

        public Vector3 torque
        {
            get { return _torque; }
            set { _torque = value; }
        }

        public Material material
        {
            get { return _material; }
            set { _material = value; }
        }

        public bool locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        public bool active
        {
            get { return _active; }
            set { _active = value; }
        }

        public float scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public GameObjectState currentState
        {
            get { return _currentState; }
        }

        public GameObjectState futureState
        {
            get { return _futureState; }
        }

        #endregion


    }
}
