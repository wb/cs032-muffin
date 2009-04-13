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
using Definitions;

namespace Muffin.Components.Renderer
{
    public class GameCamera
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

        private float _lookRotationX, _lookRotationY; // this is the rotation for looking around (separate from moving the object)

        private Vector3 _oldPosition;
        private Quaternion _oldOrientation;

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

            // these are for dealing with rotation, etc
            _oldPosition = cameraPosition;
            _lookRotationX = 0.0f;
            _oldOrientation = Quaternion.Identity;
        }

        public void updateLookRotation(float deltaRotationX, float deltaRotationY)
        {
            _lookRotationX += deltaRotationX;
            _lookRotationY += deltaRotationY;

            float maxAngle = MathHelper.ToRadians(30.0f);
            float minAngleY = MathHelper.ToRadians(15.0f);

            // this is looking left
            if (_lookRotationX > maxAngle)
                _lookRotationX = maxAngle;
            // this is looking right
            else if (_lookRotationX < -maxAngle)
                _lookRotationX = -maxAngle;

            // this is looking down
            if (_lookRotationY > minAngleY)
                _lookRotationY = minAngleY;
            // this is looking up
            else if (_lookRotationY < -maxAngle)
                _lookRotationY = -maxAngle;
        }

        public void Update(Vector3 position, Quaternion orientation)
        {
            //setTarget(position);

            // rotate the camera target by the lookRotationX amount (around the camera position)
            cameraTarget = position - cameraPosition;
            cameraTarget = Vector3.Transform(cameraTarget, Matrix.CreateFromAxisAngle(Vector3.Right, _lookRotationY) * Matrix.CreateFromAxisAngle(Vector3.Up, _lookRotationX));
            cameraTarget += cameraPosition;

            Vector3 newCameraPosition = new Vector3(0, 300, -600);
            newCameraPosition = Vector3.Transform(newCameraPosition, Matrix.CreateFromQuaternion(orientation));
            newCameraPosition += position;

            cameraPosition = 0.95f * cameraPosition + 0.05f * newCameraPosition;

            // bleed off the look angle proportionally to how much the object is moving
            Vector3 changeInPosition = position - _oldPosition;
            float bleedFromPosition = (100.0f - changeInPosition.Length()) / 100.0f;


            // bleed off the look angle proportionally to how much the object is rotation
            Quaternion changeInOrientation = orientation - _oldOrientation;
            float bleedFromOrientation = (100.0f - changeInOrientation.Length() * 250.0f) / 100.0f;

            // calculate total beled
            float totalBleed = bleedFromPosition * bleedFromOrientation;

            // bleed them off
            _lookRotationX *= totalBleed;
            _lookRotationY *= totalBleed;

            // save these for later
            _oldPosition = position;
            _oldOrientation = orientation;

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle),
                aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }

        public void setTarget(Vector3 target)
        {
            Vector3 relative_camera_shift = target - cameraTarget;
            cameraPosition += relative_camera_shift;
            cameraTarget = target;
        }

        public void zoom(int scrollFactor)
        {
            Vector3 v = cameraTarget - cameraPosition;
            v.Normalize();
            cameraPosition += (((((float)scrollFactor) / 4.0f) / 10.0f) * v);
        }
    }
}

