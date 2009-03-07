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
    class GameObject : Connectable
    {
        private Model m_model;
        private ModelType m_model_type;
        private float _mass;
        private Vector3 _position, _velocity, _acceleration, _force;
        private Quaternion _orientation;
        private Matrix _inverseIntertiaTensor;
        private Boolean _locked, _inactive;
        
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
        }

        public void addForce(Vector3 force)
        {
            _force += force;
        }

        public void updatePosition(float timestep)
        {
           
            if (!_locked)
            {
                // apply gravity
                _force += new Vector3(0, _mass * -9.8f, 0);









                // now, solve for the new position
                _acceleration += _force / _mass;
                _velocity = _velocity + _acceleration * timestep;
                _position = _position + _velocity * timestep;
                

                // account for air resistance, general drag, etc
                _velocity = 0.995f * _velocity;

                // check for collision with ground
                if (_position.Y < 24)
                {
                    _position.Y = 24;
                    _velocity.Y = -_velocity.Y;
                }
            }
            
            // reset the force on this object
            _force = Vector3.Zero;
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
                            Matrix.CreateTranslation(-(new Vector3(30.0f, 0.0f, -30.0f))) *
                            Matrix.CreateFromQuaternion(_orientation) *
                            Matrix.CreateTranslation(new Vector3(30.0f, 0.0f, -30.0f)) *
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
            

    }
}
