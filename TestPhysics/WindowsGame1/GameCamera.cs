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
    class GameCamera
    {
        public Vector3 cameraPosition { get; set; }
        public Vector3 cameraTarget { get; set; }
        public Vector3 cameraTargetNoY { get; set; }
        public Vector3 cameraUp { get; set; }

        public Vector3 AvatarHeadOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        private float aspectRatio;

        public GameCamera(Vector3 pos, Vector3 target, float aspect_ratio)
        {
            cameraPosition = pos;
            cameraTarget = target;
            cameraTargetNoY = new Vector3(target.X, 0.0f, target.Z);

            cameraUp = Vector3.Up;
            aspectRatio = aspect_ratio;
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(Vector3 rotation)
        {
            //Matrix rotationMatrixLook = Matrix.CreateTranslation(-cameraPosition)
            //                        * Matrix.CreateFromYawPitchRoll(rotation.Y, 0.0f, 0.0f)
            //                        * Matrix.CreateTranslation(cameraPosition);

            Matrix rotationMatrixPos = Matrix.CreateTranslation(-cameraTargetNoY)
                                    * Matrix.CreateFromYawPitchRoll(rotation.Y, 0.0f, 0.0f)
                                    * Matrix.CreateTranslation(cameraTargetNoY);

            //Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, 0.0f);

            //cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateTranslation(-FocusPoint));
            //cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromYawPitchRoll(rotation.Y, 0.0f, 0.0f));
            //cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateTranslation(FocusPoint)); 

            //Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(rotation.Y, 0.0f, 0.0f);

            //Vector3.Transform(cameraPosition, Matrix.CreateTranslation(FocusPoint))

            cameraPosition = Vector3.Transform(cameraPosition, rotationMatrixPos);
            cameraTarget = Vector3.Transform(cameraTarget, rotationMatrixPos);

            //Calculate the camera's view and projection matrices based on current values.
            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle),
                aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }

        public void zoom(int scrollFactor)
        {
            Vector3 v = cameraTarget - cameraPosition;
            v.Normalize();
            cameraPosition += (((float)scrollFactor / 10.0f) * v);
        }
    }
}

