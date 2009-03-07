using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1
{
    interface Connectable
    {
        void connect(GameObject obj);
        void unconnect(GameObject obj);
    }

}
