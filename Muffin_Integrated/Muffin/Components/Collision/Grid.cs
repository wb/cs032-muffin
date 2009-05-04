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
        GridCell[, ,] grids = new GridCell[4, 4, 4];
        Vector3 m_min, m_max;
        Vector3 m_stepSize;
        int counter = 0;

        public Grid(Vector3 min, Vector3 max)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        grids[i, j, k].content = new List<GameObject>();

            m_stepSize.X = (max.X - min.X) / 4f;
            m_stepSize.Y = (max.Y - min.Y) / 4f;
            m_stepSize.Z = (max.Z - min.Z) / 4f;

            m_max = max;
            m_min = min;
        }

        public void insertElement(GameObject currentO)
        {
            Vector3 index;
            index.X = (currentO.position.X - m_min.X) / m_stepSize.X;
            index.Y = (currentO.position.Y - m_min.Y) / m_stepSize.Y;
            index.Z = (currentO.position.Z - m_min.Z) / m_stepSize.Z;
            grids[(int)index.X, (int)index.Y, (int)index.Z].content.Add(currentO);
            currentO.index = index;
            counter++;
        }

        public bool moveElement(GameObject currentO)
        {
            Vector3 index;
            index.X = (currentO.position.X - m_min.X) / m_stepSize.X;
            index.Y = (currentO.position.Y - m_min.Y) / m_stepSize.Y;
            index.Z = (currentO.position.Z - m_min.Z) / m_stepSize.Z;

            if ((int)index.X != (int)currentO.index.X ||
                (int)index.Y != (int)currentO.index.Y ||
                (int)index.Z != (int)currentO.index.Z)
                return false;

            grids[(int)currentO.index.X, (int)currentO.index.Y, (int)currentO.index.Z].content.Remove(currentO);
            grids[(int)index.X, (int)index.Y, (int)index.Z].content.Add(currentO);
            currentO.index = index;
            return true;
        }

        public void removeElement(GameObject currentO)
        {
            grids[(int)currentO.index.X, (int)currentO.index.Y, (int)currentO.index.Z].content.Remove(currentO);           
        }

        public List<List<GameObject>> getList(int x, int y, int z)
        {
            List<List<GameObject>> collision = new List<List<GameObject>>();

            int minX, maxX, minY, maxY, minZ, maxZ;

            if (x == 0) minX = x;
            else minX = x - 1;

            if (y == 0) minY = y;
            else minY = y - 1;

            if (z == 0) minZ = z;
            else minZ = z - 1;

            if (x == 3) maxX = x;
            else maxX = x + 1;

            if (y == 3) maxY = y;
            else maxY = y + 1;

            if (z == 3) maxZ = z;
            else maxZ = z + 1;


            for (int i = minX; i < maxX + 1; i++)
                for (int j = minY; j < maxY + 1; j++)
                    for (int k = minZ; k < maxZ + 1; k++)
                        collision.Add(grids[i, j, k].content);

            return collision;
        }
    }
}
