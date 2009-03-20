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

namespace _3D_Renderer
{
    class GameObject
    {
        public Model m_model;
        public ModelType m_model_type;
        public ModelName m_name;

        public Vector3 Position;
        Vector3 Velocity;
        Vector3 Acceleration;
        Vector3 Force;
        public Vector3 Rotation;

        double friction;
        
        public GameObject(Model m, ModelName m_n, ModelType m_t, Vector3 pos, Vector3 rot)
        {
            //set model info
            m_model = m;
            m_model_type = m_t;
            m_name = m_n;
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

    }
}
