using System;
using System.Collections.Generic;
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
    public enum InputState
    {
        NULL,
        PRESSED,
        RELEASED,
        UNTOUCHED,
        HELD
    }

    public class InputBinding<T>
    {
        public T _input {get; set;}
        public int _timeHeld {get; set;}

        public InputBinding(T input)
        {
        _input = input;
        _timeHeld = 0;
        }

    }
}
