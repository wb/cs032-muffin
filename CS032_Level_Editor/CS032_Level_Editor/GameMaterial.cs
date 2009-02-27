using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS032_Level_Editor
{
    class GameMaterial
    {
        double _friction, _restitution;

        public GameMaterial(double friction, double restitution)
        {
            _friction = friction;
            _restitution = restitution;
        }

        public double friction
        {
            get { return _friction; }
            set { _friction = value; }
        }

        public double restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }
    }
}
