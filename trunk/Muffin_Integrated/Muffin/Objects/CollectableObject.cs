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
using Muffin;

namespace Definitions
{
    public delegate void CollectionCallback();

    /*
     * This class represents objects that don't participate in physics because they can be
     * collected.
     * */

    public class CollectableObject : GameObject
    {
        private float _rotationOffset, _sensitivity;
        private GameObject _collectionObject; // this object is trying to collect this object
        private CollectionCallback _callback; // this function will be executed upon collection
        private static Random random = new Random((int)DateTime.Now.Millisecond);
        private Boolean _hidden;

        // constructor
        public CollectableObject(ModelName modelName, CollectionCallback callback, GameObject collectionObject, float sensitivity, Vector3 position, Vector3 dimensions, float scale, Boolean hidden) :
            base(null, ModelType.COLLECTABLE, modelName, position, Quaternion.Identity, true, dimensions, 1000.0f, scale)
        {
            // create a random rotation offset
            _rotationOffset = (float) random.NextDouble() * 0.5f * (float) Math.PI;

            // set the callback and the collection object
            _callback = callback;
            _collectionObject = collectionObject;

            _sensitivity = sensitivity;

            _hidden = hidden;
        }

        public void updateOrientation(GameTime gameTime)
        {
            // no need to update if this object is hidden
            if (!_hidden)
            {
                float angle = MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds / 5.0f);
                _orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle + _rotationOffset);
            }
        }

        /*
         * Override world matrix, because we need to use orientation.
         * */

        public override Matrix worldMatrix()
        {
            if (!_hidden)
            {
                return Matrix.CreateFromQuaternion(_orientation) *
                       Matrix.CreateScale(_scale) *
                       Matrix.CreateTranslation(_futureState.position * _scale);
            }
            else
            {
                return Matrix.CreateFromQuaternion(_orientation) *
                       Matrix.CreateScale(0.0f) *
                       Matrix.CreateTranslation(_futureState.position * 0.0f);
            }
        }

        /*
         * This method checks to see if the registered object has picked it up.  If so,
         * it performs the appropriate function and returns true.
         * */

        public void checkForCollection(MuffinGame game)
        {
            // make sure this function only gets called once (and doesnt get called if it is hidden)
            if (_toBeRemoved || _hidden)
                return;

            // if its within range
            if (Math.Abs(position.X - _collectionObject.position.X) < _sensitivity && Math.Abs(position.Y - _collectionObject.position.Y) < _sensitivity && Math.Abs(position.Z - _collectionObject.position.Z) < _sensitivity)
            {
                // execute the callback
                if(_callback != null)
                    _callback();

                // flag as removeable
                _toBeRemoved = true;

                // flag as updated
                game.addUpdateObject(this);

            }
        }

        /*
         * This is used to hide an object (such as a star) before it is time to use it.
         * */

        public Boolean hidden
        {
            get { return _hidden; }
            set { _hidden = value; }
        }
    }
}
