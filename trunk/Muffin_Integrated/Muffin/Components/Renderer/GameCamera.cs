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

        private float _zoom, aspectRatio, _lookRotationX, _lookRotationY; // this is the rotation for looking around (separate from moving the object)

        private Vector3 _oldPosition, _relativeCameraPosition;
        private Quaternion _oldOrientation;

        public GameCamera(Vector3 pos, Vector3 target, float aspect_ratio)
        {
            cameraPosition = pos;
            cameraTarget = target;

            _relativeCameraPosition = new Vector3(0, 150, -300);
            _zoom = 1.5f;

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

            // rotate up around this axis:
            Vector3 verticalLookRotationVector = Vector3.Right;
            verticalLookRotationVector = Vector3.Transform(verticalLookRotationVector, Matrix.CreateFromQuaternion(orientation));

            // the camera target will normally be the object, but can sometimes be elsewhere if you look around
            cameraTarget = Vector3.Transform(cameraTarget, Matrix.CreateFromAxisAngle(verticalLookRotationVector, _lookRotationY) * Matrix.CreateFromAxisAngle(Vector3.Up, _lookRotationX));
            cameraTarget += cameraPosition;

            Vector3 newCameraPosition = _relativeCameraPosition * _zoom;
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

        public void zoom(float scrollFactor)
        {
            float lowerBound = 0.5f;
            float upperBound = 5.0f;

            _zoom -= scrollFactor / 500.0f;

            if (_zoom < lowerBound)
                _zoom = lowerBound;
            else if (_zoom > upperBound)
                _zoom = upperBound;

        }
    }
}

