using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Definitions;

namespace Muffin.Components.Collision
{
    public struct GridCell
    {
        public List<GameObject> content;
    }

    public class Grid
    {
        GridCell[, ,] grids = new GridCell[12, 12, 12];
        Vector3 m_min, m_max;
        Vector3 m_stepSize;

        public Grid(Vector3 min, Vector3 max)
        {
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                    for (int k = 0; k < 12; k++)
                        grids[i, j, k].content = new List<GameObject>();

            m_stepSize.X = (max.X - min.X) / 10f;
            m_stepSize.Y = (max.Y - min.Y) / 10f;
            m_stepSize.Z = (max.Z - min.Z) / 10f;

            m_max = max;
            m_min = min;
        }

        public void insertElement(GameObject currentO)
        {
            Vector3 index;
            index.X = (currentO.position.X - m_min.X) / m_stepSize.X;
            index.Y = (currentO.position.Y - m_min.Y) / m_stepSize.Y;
            index.Z = (currentO.position.Z - m_min.Z) / m_stepSize.Z;
            grids[(int)index.X + 1, (int)index.Y + 1, (int)index.Z + 1].content.Add(currentO);
            currentO.index = index;
        }

        public bool moveElement(GameObject currentO)
        {
            Vector3 index;
            index.X = (currentO.position.X - m_min.X) / m_stepSize.X;
            index.Y = (currentO.position.Y - m_min.Y) / m_stepSize.Y;
            index.Z = (currentO.position.Z - m_min.Z) / m_stepSize.Z;

            if ((int)index.X == (int)currentO.index.X &&
               (int)index.Y == (int)currentO.index.Y &&
               (int)index.Z == (int)currentO.index.Z)
                return false;

            grids[(int)currentO.index.X+1, (int)currentO.index.Y+1, (int)currentO.index.Z+1].content.Remove(currentO);
            grids[(int)index.X+1, (int)index.Y+1, (int)index.Z+1].content.Add(currentO);
            currentO.index = index;
            return true;
        }

        public List< List<GameObject> > getList(int x, int y, int z)
        {
            List< List<GameObject> > collision = new List< List<GameObject> >();

            for (int i = x - 1; i < x + 2; i++)
                for (int j = y - 1; j < y + 2; j++)
                    for (int k = z - 1; k < z + 2; k++)
                        collision.Add(grids[i+1, j+1, k+1].content);

            return collision;
        }
    }
}
