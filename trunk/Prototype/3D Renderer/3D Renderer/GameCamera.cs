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
    class GameCamera
    {
        public Vector3 cameraPosition { get; set; }
        public Vector3 cameraTarget { get; set; }
        public Vector3 cameraUp { get; set; }

        public Vector3 AvatarHeadOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        private float aspectRatio;
        private float current_y;
        private float min_y = 5.0f, max_y = 70.0f;

        public GameCamera(Vector3 pos, Vector3 target, float aspect_ratio)
        {
            cameraPosition = pos;
            cameraTarget = target;

            Vector3 noY = new Vector3(cameraTarget.X, 0.0f, cameraTarget.Z);           
            Vector3 temp = Vector3.Transform(cameraPosition, Matrix.CreateTranslation(-noY));

            double radius = Math.Sqrt(Math.Pow(temp.X, 2) + Math.Pow(temp.Z, 2));
            double distance = (cameraPosition - cameraTarget).Length();
            current_y = MathHelper.ToDegrees((float)Math.Acos(radius / distance));
            Console.WriteLine(current_y);
            cameraUp = Vector3.Up;
            aspectRatio = aspect_ratio;
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(Vector3 rotation)
        {
            Matrix rotMat;
            bool includeXRot = true;
            
            float degree_y = MathHelper.ToDegrees(rotation.X);
            if (current_y - degree_y > max_y) {
                current_y = max_y;
                includeXRot = false;
            } else if (current_y - degree_y < min_y) {
                current_y = min_y;
                includeXRot = false;
            } else {
                current_y -= degree_y;
            }

            if(includeXRot) {
                rotMat = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, 0.0f);
            } else {
                rotMat = Matrix.CreateFromYawPitchRoll(rotation.Y, 0.0f, 0.0f);
            }

            Matrix rotationMatrixPos = Matrix.CreateTranslation(-cameraTarget)
                                    * rotMat
                                    * Matrix.CreateTranslation(cameraTarget);

            cameraPosition = Vector3.Transform(cameraPosition, rotationMatrixPos);

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle),
                aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }

        public void setTarget(Vector3 target) {
            Vector3 relative_camera_shift = target - cameraTarget;
            cameraPosition += relative_camera_shift;
            cameraTarget = target;
        }

        public void zoom(int scrollFactor)
        {
            Vector3 v = cameraTarget - cameraPosition;
            v.Normalize();
            cameraPosition += (((((float)scrollFactor)/4.0f) / 10.0f) * v);
        }
    }
}

