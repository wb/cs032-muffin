using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS032_Level_Editor
{
    class GameMaterial
    {
        float _friction, _restitution;

        public GameMaterial(float friction, float restitution)
        {
            _friction = friction;
            _restitution = restitution;
        }

        public float friction
        {
            get { return _friction; }
            set { _friction = value; }
        }

        public float restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }

        /**
         * Some common materials to allow for easier access.
         * 
         * Note: These are made-up values.  They can be tweaked during
         * testing for most fun or most accurate response.
         * 
         **/

        public static readonly GameMaterial wood = new GameMaterial(0.1f, 0.2f);
        public static readonly GameMaterial grass = new GameMaterial(0.4f, 0.5f);
    }
}
