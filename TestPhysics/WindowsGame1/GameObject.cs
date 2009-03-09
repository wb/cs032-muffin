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
using WindowsGame1;

namespace WindowsGame1
{
    class GameObject : Connectable, Physics.PhysicsObject
    {
        private Model m_model;
        private ModelType m_model_type;
        private Physics.Material _material;
        private float _mass;
        private Vector3 _position, _velocity, _acceleration, _force, _centerOfMass, _angularVelocity, _angularAcceleration, _torque;
        private Quaternion _orientation;
        private Matrix _intertiaTensor;
        private Boolean _locked, _active;
        private List<Physics.CollisionRegion> _collisionRegions;
        
        public GameObject(Model m, ModelType m_t, Vector3 pos, Quaternion rot, Boolean locked)
        {
            //set model info
            m_model = m;
            m_model_type = m_t;
            //set model orientation
            _position = pos;
            _orientation = rot;
            //initialize rest of the parameters
            _velocity = new Vector3();
            _acceleration = new Vector3();
            _force = new Vector3();
            _locked = locked;
            _mass = 100;

            _centerOfMass = new Vector3(30, 30, 30);

            _collisionRegions = new List<Physics.CollisionRegion>();

            _intertiaTensor = new Matrix(1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 0);
        }

        public GameObject(Model m, ModelType m_t, Vector3 pos, Quaternion rot)
        {
            //set model info
            m_model = m;
            m_model_type = m_t;

            //set model placement
            _position = pos;
            _orientation = rot;
      
            //initialize rest of the parameters
            _velocity = new Vector3();
            _acceleration = new Vector3();
            _force = new Vector3();
            _locked = false;
            _mass = 100;

            _collisionRegions = new List<Physics.CollisionRegion>();

            _intertiaTensor = new Matrix(1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 1.0f / (.385f * _mass), 0, 0, 0, 0, 0);

            _centerOfMass = new Vector3(30, 30, 30);
        }

        public void addForce(Vector3 force)
        {
            _force += force;
        }

        public void renderObject()
        {
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in m_model.Meshes)
            {
                // This is where the mesh orientation is set,
                // (Camera and Projection are set once within the main rendering class per render step)
                foreach (BasicEffect effect in mesh.Effects)
                {
   
                    effect.World = 
                            //Matrix.CreateTranslation(-(new Vector3(30.0f, 0.0f, -30.0f))) *
                            Matrix.CreateFromQuaternion(_orientation) *
                            //Matrix.CreateTranslation(new Vector3(30.0f, 0.0f, -30.0f)) *
                            Matrix.CreateTranslation(_position);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

        }

        public void connect(GameObject obj)
        {
        }

        public void unconnect(GameObject obj)
        {
        }



        #region PhysicsObject Members

        public float mass
        {
            get
            {
                return _mass;
            }
            set
            {
                _mass = value;
            }
        }

        public Vector3 position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public Vector3 velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }

        public Vector3 acceleration
        {
            get
            {
                return _acceleration;
            }
            set
            {
                _acceleration = value;
            }
        }

        public Vector3 force
        {
            get
            {
                return _force;
            }
            set
            {
                _force = value;
            }
        }

        public Vector3 centerOfMass
        {
            get
            {
                return _centerOfMass;
            }
            set
            {
                _centerOfMass = value;
            }
        }

        public Quaternion orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                _orientation = value;
            }
        }

        public Vector3 angularVelocity
        {
            get
            {
                return _angularVelocity;
            }
            set
            {
                _angularVelocity = value;
            }
        }

        public Vector3 angularAcceleration
        {
            get
            {
                return _angularAcceleration;
            }
            set
            {
                _angularAcceleration = value;
            }
        }

        public Vector3 torque
        {
            get
            {
                return _torque;
            }
            set
            {
                _torque = value;
            }
        }

        public Physics.Material material
        {
            get
            {
                return _material;
            }
            set
            {
                _material = value;
            }
        }

        public bool locked
        {
            get
            {
                return _locked;
            }
            set
            {
                _locked = value;
            }
        }

        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        // this is the location with respect to the center of the object
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

            // do the integration

            if (!_locked)
            {
                // first, solve for the new rotational position (orientation)
                Vector3 temp = new Vector3();
                
                temp = Vector3.Transform(_torque, _intertiaTensor);

                _angularAcceleration += temp;

                _angularVelocity = _angularVelocity + _angularAcceleration * timestep;

                Quaternion deltaOrientation = Quaternion.CreateFromAxisAngle(Vector3.Right, _angularVelocity.X * timestep) * Quaternion.CreateFromAxisAngle(Vector3.Up, _angularVelocity.Y * timestep) * Quaternion.CreateFromAxisAngle(Vector3.Backward, _angularVelocity.Z * timestep);

                deltaOrientation.Normalize();

                _orientation *= deltaOrientation;

                // now, solve for the new position
                _acceleration += _force / _mass;
                _velocity = _velocity + _acceleration * timestep;
                _position = _position + _velocity * timestep;
                
                // account for air resistance, general drag, etc
                _velocity = 0.995f * _velocity;
                _angularVelocity = 0.995f * _angularVelocity;

                
               

                /*
                // check for collision with ground
                if (one.Y < 24 || two.Y < 24 || three.Y < 24 || four.Y < 24 || five.Y < 24 || six.Y < 24 || seven.Y < 24 || eight.Y < 24)
                {
                    _position.Y += 1;
                    _velocity.Y = -_velocity.Y;
                }
                 * 
                 * */
            }

            this.postPhysics();
        }

        public List<Physics.CollisionRegion> collisionRegions
        {
            get { return _collisionRegions; }
        }

        public void addCollision(Physics.CollisionRegion collision)
        {
            _collisionRegions.Add(collision);
        }

        public void prePhysics()
        {
            throw new NotImplementedException();
        }

        public void postPhysics()
        {
            // reset the force and torque to zero
            _force = Vector3.Zero;
            _torque = Vector3.Zero;

            // clear all collision regions
            _collisionRegions.Clear();
        }

        #endregion
    }
}
