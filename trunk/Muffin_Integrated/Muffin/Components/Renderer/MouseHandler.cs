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
using Definitions;

namespace Muffin.Components.Renderer
{
    class MouseHandler
    {
        MouseState m_state_old;
        MouseState m_state_current;

        public MouseHandler(MouseState m)
        {
            m_state_current = m;
        }

        public void updateHandler(MouseState m)
        {
            m_state_old = m_state_current;
            m_state_current = m;
        }

        public float getNetX()
        {
            return (m_state_current.X - m_state_old.X);
        }

        public float getNetY()
        {
            return (m_state_current.Y - m_state_old.Y);
        }

        public Boolean shouldRotate()
        {
            return (m_state_current.RightButton == ButtonState.Pressed && m_state_old.RightButton == ButtonState.Pressed);
        }

        public int getNetScroll()
        {
            int scroll = m_state_current.ScrollWheelValue - m_state_old.ScrollWheelValue;
            return m_state_current.ScrollWheelValue - m_state_old.ScrollWheelValue;
        }

    }
}
