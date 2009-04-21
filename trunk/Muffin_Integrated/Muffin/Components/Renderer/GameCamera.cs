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

        private Boolean _lookMode;

        private int _lookModeCount = 0;

        public GameCamera(Vector3 pos, Vector3 target, float aspect_ratio)
        {
            cameraPosition = pos;
            cameraTarget = target;

            _relativeCameraPosition = GameConstants.GameObjectScale * new Vector3(0, 150, -300);
            _zoom = 1.5f;

            cameraUp = Vector3.Up;
            aspectRatio = aspect_ratio;
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;

            // these are for dealing with rotation, etc
            _oldPosition = cameraPosition;
            _lookRotationX = 0.0f;
            _oldOrientation = Quaternion.Identity;

            _lookMode = false;
        }

        public void updateLookRotation(float deltaRotationX, float deltaRotationY)
        {
            
            if (Math.Abs(deltaRotationX) > 0.0f)
            {
                _lookMode = true;
                _lookModeCount = 0;

                if (Math.Abs(deltaRotationX) < 0.01f)
                    deltaRotationX = 0.0f;
            }
            else
            {
                _lookModeCount++;

                if (_lookModeCount > 45)
                    _lookMode = false;
            }

            Console.WriteLine(deltaRotationX);
            _lookRotationX -= 2.0f * deltaRotationX;
            _lookRotationY -= 2.0f * deltaRotationY;

            float pi = (float)Math.PI;

            // this code will cause a wrap-around to occur at larger angles, so that the camera
            // always travels the minimum distance back to the objects orientation
            if (_lookRotationX > pi)
                _lookRotationX -= 2.0f * pi;
            else if (_lookRotationX < -pi)
                _lookRotationX += 2.0f * pi;
            
        }

        public void Update(Vector3 position, Quaternion orientation)
        {
            position = GameConstants.GameObjectScale * position;

            // rotate the camera target by the lookRotationX amount (around the camera position)
            cameraTarget = position;

            Vector3 newCameraPosition = _relativeCameraPosition * _zoom;
            newCameraPosition = Vector3.Transform(newCameraPosition, Matrix.CreateFromQuaternion(orientation) * Matrix.CreateFromAxisAngle(Vector3.Up, _lookRotationX));
            newCameraPosition += position;

            cameraPosition = 0.95f * cameraPosition + 0.05f * newCameraPosition;

            // bleed off the look angle proportionally to how much the object is moving
            Vector3 changeInPosition = position - _oldPosition;
            float bleedFromPosition = (50.0f - (changeInPosition.Length() / GameConstants.GameObjectScale)) / 50.0f;


            // bleed off the look angle proportionally to how much the object is rotation
            Quaternion changeInOrientation = orientation - _oldOrientation;
            float bleedFromOrientation = (50.0f - changeInOrientation.Length() * 250.0f) / 50.0f;

            // calculate total beled
            float totalBleed = bleedFromPosition * bleedFromOrientation;

            if (totalBleed > 0)
            {
                totalBleed = (float)Math.Sqrt(totalBleed);
            }
            // bleed them off if X is the reset camera button was pressed

            if (!_lookMode)
            {
                _lookRotationX *= totalBleed;
                _lookRotationY *= totalBleed;
            }

            // save these for later
            _oldPosition = position;
            _oldOrientation = orientation;

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle),
                aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }

        public void zoom(float scrollFactor)
        {
            float lowerBound = 0.8f;
            float upperBound = 5.0f;

            _zoom -= scrollFactor / 500.0f;

            if (_zoom < lowerBound)
                _zoom = lowerBound;
            else if (_zoom > upperBound)
                _zoom = upperBound;

        }

        /*
         * This will toggle the look mode between
         * auto following where the object is oriented
         * and allowing the user to choose a viewing angle.
         * */

        public void lookMode(Boolean change)
        {
            /*if (change)
                _lookMode = !_lookMode;*/
        }
    }
}

