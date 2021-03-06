﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.IO;

namespace CS032_Level_Editor
{
    public enum ModelType { TERRAIN, ENEMY, OBJECT, HUMAN };

    class GameObject
    {
        private Model _model;
        private ModelType _modelType;

        // units in: meters, meters per second, meters per second squared, newtons, meters, and radians
        private Vector3 _position, _velocity, _acceleration, _force, _centerOfMass, _rotation;
        private float _mass;
        
        // The material contains properties like friction and coefficient of restitution.
        private GameMaterial _material;
        
        // Locked means the object is locked and can never move (such as ground)
        // Inactive means it hasn't moved in a while and therefore doesn't need to be checked for collisions
        // (though other objects need to be collided with it). This will be computed during the dynamics portion
        // of the physics computation and updated as necessary.
        private Boolean _locked, _inactive;

        // The collisionregions list contains all of the collisions for this round of calculations
        private List<CollisionRegion> _collisionRegions;

        public GameObject(Model model, ModelType modelType, Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 rotation, float mass, GameMaterial material, Boolean locked)
        {
            _model = model;
            _modelType = modelType;
            _position = position;
            _velocity = velocity;
            _acceleration = acceleration;
            _rotation = rotation;
            _mass = mass;
            _material = material;
            _locked = locked;

            // force set to 0 initially -- will be computed by the physics engine
            _force = Vector3.Zero;

            // object should be active initially
            _inactive = false;

            // create a new list of collision regions
            _collisionRegions = new List<CollisionRegion>();
        }

        public void draw()
        {
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in _model.Meshes)
            {
                // This is where the mesh orientation is set,
                // (Camera and Projection are set once within the main rendering class per render step)
                foreach (BasicEffect effect in mesh.Effects)
                {
                    Matrix rotY = Matrix.CreateRotationY(_rotation.Y);
                    effect.World =
                            Matrix.CreateTranslation(-(new Vector3(30.0f, 0.0f, -30.0f))) *
                            Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z) *
                            Matrix.CreateTranslation(new Vector3(30.0f, 0.0f, -30.0f)) *
                            Matrix.CreateTranslation(_position);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

        }

        /**
         * Methods relating to collisions.
         * 
         **/

        public List<CollisionRegion> getCollisions()
        {
            return _collisionRegions;

        }

        public void addCollision(CollisionRegion collision)
        {
            _collisionRegions.Add(collision);
        }

        /**
         * This method resets all of the fields after each round of calculation is done.
         * 
         **/

        public void reset()
        {
            _collisionRegions.Clear();
            _force = Vector3.Zero;
        }

        /**
         * All of the gets and sets.
         * 
         **/

        public Model model
        {
            get { return _model; }
            set { _model = value; }
        }

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

        public Vector3 rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float mass
        {
            get { return _mass; }
            set { _mass = value; }
        }

        public Boolean locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        public Boolean inactive
        {
            get { return _inactive; }
            set { _inactive = value; }
        }

        public GameMaterial material
        {
            get { return _material; }
            set { _material = value; }
        }

    }
}
