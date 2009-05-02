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
using Muffin;

namespace Definitions
{
    public class GameObject
    {
        /*
         * Please keep all of these private! If you need to access
         * one of them that doesn't have a get/set, please add a
         * get/set in the appropriate place at the end of this file.
         * */

        protected Model _model;
        protected ModelType _modelType;
        protected Material _material;
        protected float _mass, _scale;
        protected ModelName _modelName;
        protected Vector3 _force, _centerOfMass, _torque, _dimensions, _toMove, _previousToMove, _moveVector,_index;
        protected Matrix _intertiaTensor;
        protected Boolean _locked, _active, _toBeRemoved;
        protected BoundingBox _boundingBox;
        protected GameObjectState _previousState, _currentState, _futureState;
        protected Quaternion _orientation; // this is used for the direction an object is facing (for moving, camera, etc).
        protected int _jumpCount;

        /*
         * This is the constructor.  
         * */

        public GameObject(Model model, ModelType modelType, ModelName modelName, Vector3 position, Quaternion rotation, Boolean locked, Vector3 dimensions, float mass, float scale)
        {
            // initialize the game state objects (the future one should be the current one for now)
            _currentState = new GameObjectState(position, rotation);
            _futureState = new GameObjectState(position, rotation);
            _previousState = new GameObjectState(position, rotation);

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

            _toMove = new Vector3();
            _previousToMove = _toMove;

            // initalize the orientation
            _orientation = Quaternion.Identity;

            _toBeRemoved = false;
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
         * This method is used to take input and translate it into movement.
         * */

        public virtual void move(float upDown, float leftRight, float strafeValue, Boolean jump, Boolean strafe)
        {

        }
        /*
         * This method is used to update the bounding box whenever the object has moved.
         * */

        public void updateBoundingBox()
        {
            // the min and max are (0,0,0) and (D.x, D.y, D.z) where D is the dimension vector
            Vector3 min, max;

            min = -0.5f * _dimensions;
            max = 0.5f * _dimensions;

            // now we need to transform these using the world matrix
            min = Vector3.Transform(min, Matrix.CreateFromQuaternion(_futureState.rotation) * Matrix.CreateTranslation(_futureState.position));
            max = Vector3.Transform(max, Matrix.CreateFromQuaternion(_futureState.rotation) * Matrix.CreateTranslation(_futureState.position));

            // create a new bounding box
            _boundingBox = new BoundingBox(min, max);

        }

        public virtual BoundingBox getCurrentBoundingBox()
        {

            // the min and max are (0,0,0) and (D.x, D.y, D.z) where D is the dimension vector
            Vector3 min, max;

            min = -0.5f * _dimensions;
            max = 0.5f * _dimensions;

            // now we need to transform these using the world matrix
            min = Vector3.Transform(min, Matrix.CreateFromQuaternion(_currentState.rotation) * Matrix.CreateTranslation(_currentState.position));
            max = Vector3.Transform(max, Matrix.CreateFromQuaternion(_currentState.rotation) * Matrix.CreateTranslation(_currentState.position));

            // create a new bounding box
            return new BoundingBox(min, max);

        }

        /*
         * This method is used to correct for small errors in the bounding box and is used
         * for all active objects during interpenetration resolution.  The problem we were 
         * having was that bounding boxes that are simply touching aren't, according to XNA,
         * colliding.  This allows for touching to be considered as colliding by enlarging this
         * bounding box slightly.
         * */

        public virtual BoundingBox getCurrentBoundingBoxEnlarged()
        {
            // the min and max are (0,0,0) and (D.x, D.y, D.z) where D is the dimension vector
            Vector3 min, max;

            float scaleFactor = 1.003f;

            min = -0.5f * _dimensions * scaleFactor;
            max = 0.5f * _dimensions * scaleFactor;

            // now we need to transform these using the world matrix
            min = Vector3.Transform(min, Matrix.CreateFromQuaternion(_currentState.rotation) * Matrix.CreateTranslation(_currentState.position));
            max = Vector3.Transform(max, Matrix.CreateFromQuaternion(_currentState.rotation) * Matrix.CreateTranslation(_currentState.position));

            // create a new bounding box
            return new BoundingBox(min, max);
        }

        /*
         * This method returns the world matrix.  For any object
         * that extends game object, such as TerrainObject,
         * worldMatrix will be stored so that it does not
         * have to be recalculated each and every timestep.
         * */

        public virtual Matrix worldMatrix()
        {
            return Matrix.CreateFromQuaternion(_currentState.rotation) *
                   Matrix.CreateScale(_scale) *
                   Matrix.CreateTranslation(_currentState.position * GameConstants.GameObjectScale);
        }

        /*
         * This method returns the future world matrix.  It is used
         * during collision detection, resolution, and the construction
         * of a new bounding box.
         * */

        public virtual Matrix futureWorldMatrix()
        {
            return Matrix.CreateFromQuaternion(_futureState.rotation) *
                   Matrix.CreateScale(_scale) *
                   Matrix.CreateTranslation(_futureState.position * GameConstants.GameObjectScale);
        }

        /*
         * This method is used to apply force to the object at
         * a given location.
         * */

        public void applyForce(Vector3 force, Vector3 location)
        {
            // Error Checking: make sure this force wont make the total acceleration too large
            float acceleration = ((_currentState.acceleration * _mass + force) / mass).Length();
            if (acceleration < GameConstants.MaxAcceleration && -acceleration > -GameConstants.MaxAcceleration)
            {
                // add this force on to the linear force
                _force += force;

                // the location is the location, so to find torque, find the cross product of (location - centerOfMass) and force
                location = location - _centerOfMass;

                Vector3.Cross(location, force);
                _torque += location;
            }
        }

        /*
         * This method applies a force on the center of the object.
         * */

        public void applyForceAtCenter(Vector3 force)
        {

            // Error Checking: make sure this force wont make the total acceleration too large
            float acceleration = ((_currentState.acceleration * _mass + force) / mass).Length();
            if (acceleration < GameConstants.MaxAcceleration && -acceleration > -GameConstants.MaxAcceleration)
            {
                // add this force on to the linear force
                _force += force;
            }
        }

        public void integrate(float timestep)
        {


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

                // check for max position

                if (_futureState.position.Y > GameConstants.MaxHeight)
                {
                    _futureState.position = new Vector3(_futureState.position.X, GameConstants.MaxHeight, _futureState.position.Z);
                    _futureState.acceleration = new Vector3(_futureState.acceleration.X, 0, _futureState.acceleration.Z);
                    _futureState.velocity = new Vector3(_futureState.velocity.X, 0, _futureState.velocity.Z);
                }

                // check for min position
                float minPosition = (this is PlayerObject ? GameConstants.MinHeightPlayer : GameConstants.MinHeightObject);

                if (_futureState.position.Y < minPosition)
                {
                    _toBeRemoved = true;

                    _futureState.position = new Vector3(_futureState.position.X, minPosition, _futureState.position.Z);
                    _futureState.acceleration = new Vector3(_futureState.acceleration.X, 0, _futureState.acceleration.Z);
                    _futureState.velocity = new Vector3(_futureState.velocity.X, 0, _futureState.velocity.Z);

                  
                }

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

        public void prePhysics(MuffinGame game)
        {
            // update the bounding box if we've moved (during collision resolution)
            if (!_locked)
            {
                // get the move vector -- cap it for safety
                _moveVector = _currentState.position - _previousState.position;
                float length, maxSpeed = 1.0f;
                if ((length = _moveVector.Length()) > maxSpeed)
                {
                    _moveVector *= (maxSpeed / length);
                }

                // update bounding box if needed (and alert donnie)
                if (_previousState.position != _currentState.position)
                {
                    game.addUpdateObject(this);
                    this.updateBoundingBox();
                }

                // copy the current state to previous state
                _previousState.copy(_currentState);

               
            }
              
        }

        /*
         * This method is called everytime that physics finishes an
         * iteration. It is used to clean up, set forces/torques
         * to zero, etc.
         * */

        public void postPhysics(Boolean move, MuffinGame game)
        {
            // reset the force and torque to zero
            _force = Vector3.Zero;
            _torque = Vector3.Zero;

            if (move)
            {
                
                // update the position
                _currentState.copy(_futureState);
            }
            else
            {
                // otherwise, reset the future state (we didn't move)
                _futureState.copy(_currentState);
            }

            // this is friction
            
            Vector3 adjustedMove;

            if (_modelType == ModelType.ENEMY || _modelType == ModelType.HUMAN)
            {
                float friction = 0.90f;
                _toMove *= 2.5f;
                adjustedMove = friction * _previousToMove + (1.0f - friction) * _toMove;
            }
            else
            {
                float friction = 0.98f;
                //_toMove *= 50.0f;
                adjustedMove = friction * (new Vector3(_moveVector.X, 0, _moveVector.Z)) + (1.0f - friction) * _toMove;
            }
            
            
            // move the object
            _futureState.position = _futureState.position + adjustedMove; // this line stops physics from working
            _currentState.position = _currentState.position + adjustedMove;

            // save this to the previous move state
            _previousToMove = adjustedMove;

            // reset the move vector
            _toMove = Vector3.Zero;

           
        }

        #region Gets and Sets

        /*
         * Below are all of the gets and sets.  Please use gets and setes
         * instead of making variables public if you need to access them.
         * Follow the naming scheme below by using the variable name without
         * the preceeding underscore.
         * */
        public Vector3 index
        {
            get { return _index; }
            set { _index = value; }
        }

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

        /*
         * If this object is locked, its mass is essentially infinite, so
         * return the largest float.
         * */

        public float mass
        {
            get
            {
                if (_locked)
                    return float.MaxValue;
                else
                    return _mass;
            }
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

        public GameObjectState previousState
        {
            get { return _previousState; }
        }

        public Quaternion orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public Vector3 moveVector
        {
            get { return _moveVector; }
            set { _moveVector = value; }
        }

        public Boolean toBeRemoved
        {
            get { return _toBeRemoved; }
        }

        public int jumpCount
        {
            get { return _jumpCount; }
            set { _jumpCount = value; }
        }

        #endregion

    }
}
