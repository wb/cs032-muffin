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
        Model m_model;
        ModelType m_model_type;

        public Vector3 Position;
        Vector3 Velocity;
        Vector3 Acceleration;
        Vector3 Force;
        public Vector3 Rotation;

        double friction;
        
        public GameObject(Model m, ModelType m_t, Vector3 pos, Vector3 rot)
        {
            //set model info
            m_model = m;
            m_model_type = m_t;
            //set model orientation
            Position = pos;
            Rotation = new Vector3();
            Rotation.X = MathHelper.ToRadians(rot.X);
            Rotation.Y = MathHelper.ToRadians(rot.Y);
            Rotation.Z = MathHelper.ToRadians(rot.Z);
            //initialize rest of the parameters
            Velocity = new Vector3();
            Acceleration = new Vector3();
            Force = new Vector3();
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
                    Matrix rotY = Matrix.CreateRotationY(Rotation.Y);
                    effect.World = 
                            Matrix.CreateTranslation(-(new Vector3(30.0f, 0.0f, -30.0f))) *
                            Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                            Matrix.CreateTranslation(new Vector3(30.0f, 0.0f, -30.0f)) *
                            Matrix.CreateTranslation(Position);
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
