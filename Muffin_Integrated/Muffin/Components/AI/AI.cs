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

namespace Muffin.Components.AI
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class AI : Microsoft.Xna.Framework.GameComponent
    {
        private MuffinGame m_game;
        private WeightedGraph<TerrainObject> m_world;
        private SortedList<float, GameObject>[,] m_grid;
        private Dictionary<GameObject, Point> m_index;

        public MuffinGame game { get { return m_game; } }

        #region General External Methods

        public GameObject topmostObject(float X, float Y)
        {
            int gridX = (int)(X / 60f);
            int gridY = (int)(Y / 60f);

            return topmostObjectGrid(gridX, gridY);
        }

        public TerrainObject topmostTerrain(float X, float Y)
        {
            int gridX = (int)(X / 60f);
            int gridY = (int)(Y / 60f);

            return topmostTerrainGrid(gridX, gridY);

        }

        #endregion

        #region Internal Methods

        public AI(Game game)
            : base(game)
        {
            m_game = (MuffinGame)game;
            m_world = new WeightedGraph<TerrainObject>();
            m_index = new Dictionary<GameObject, Point>();
        }

        private GameObject topmostObjectGrid(int X, int Y)
        {
            if (m_grid[X, Y] != null)
            {
                return m_grid[X, Y].Values[m_grid[X, Y].Count - 1];
            }
            else
                return null;
        }

        private TerrainObject topmostTerrainGrid(int X, int Y)
        {
            GameObject o = topmostObjectGrid(X, Y);
            if (o is TerrainObject)
            {
                return (TerrainObject)o;
            }
            else
                if (o == null)
                    return null;
                else
                {
                    int idx = m_grid[X,Y].Count - 2;
                    o = m_grid[X, Y].Values[idx];
                    while (!(o is TerrainObject) && (idx > 0) )
                        o = m_grid[X, Y].Values[--idx];

                    if (!(o is TerrainObject))
                        return null;
                    else
                        return (TerrainObject)o;
                }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            int maxX = 0, maxY = 0;

            // First, build the grid
            foreach (TerrainObject t in m_game.allTerrain)
            {
                if (t.gridX > maxX)
                    maxX = t.gridX;
                if (t.gridY > maxY)
                    maxY = t.gridY;
            }
            maxX++; maxY++;

            m_grid = new SortedList<float, GameObject>[maxX, maxY];
            foreach (GameObject o in m_game.allObjects)
            {
                // Add all objects except AIObjects and PlayerObjects to the grid
                if (!(o is AIObject || o is PlayerObject || o is CollectableObject))
                {
                    int gridX, gridY;
                    if (o is TerrainObject)
                    {
                        gridX = ((TerrainObject)o).gridX;
                        gridY = ((TerrainObject)o).gridY;
                    }
                    else
                    {
                        gridX = (int)(o.position.X / 60.0f);
                        gridY = (int)(o.position.Z / 60.0f);
                    }

                    if (m_grid[gridX, gridY] == null)
                        m_grid[gridX, gridY] = new SortedList<float, GameObject>();

                    m_grid[gridX, gridY].Add(o.position.Y, o);
                    m_index.Add(o, new Point(gridX, gridY));
                }
            }

            // Then, build the graph from the grid
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    TerrainObject o1 = topmostTerrainGrid(x, y);
                    if(o1 != null)
                        for(int x2 = Math.Max(x-1,0); x2 < Math.Min(x + 2, maxX); x2++)
                            for (int y2 = Math.Max(y - 1, 0); y2 < Math.Min(y + 2, maxY); y2++)
                            {
                                if (x2 == x && y2 == y)
                                    continue;

                                TerrainObject o2 = topmostTerrainGrid(x2, y2);
                                if (o2 != null)
                                {
                                    // Make sure this edge is valid
                                    // Right now the AI can't jump, so any sudden upward gradient is impassable
                                    // Falling down is ok, to a point...
                                    // TODO: How to tell ramps apart from non-ramps simply?
                                    int dist = (int)Math.Floor(o1.position.Y - o2.position.Y);
                                    if (dist >= 0 && dist < GameConstants.MaxFallDistance)
                                    {
                                        // Currently the difference in height (to the nearest int) is the weight
                                        m_world.SetEdge(o1, o2, dist);
                                    }
                                }
                            }
                }
            }

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Remove all objects from the grid that are gone
            foreach (GameObject o in m_game.removed)
            {
                if (o is AIObject || o is PlayerObject)
                    continue;

                Point pos;
                if (m_index.TryGetValue(o, out pos))
                {
                    SortedList<float, GameObject> l = m_grid[pos.X, pos.Y];
                    l.RemoveAt(l.IndexOfValue(o));
                    m_index.Remove(o);
                }

            }

            // Update grid based on objects that have moved
            foreach (GameObject o in m_game.updated)
            {
                if (o is AIObject || o is PlayerObject)
                    continue;

                Point oldPos;
                if (m_index.TryGetValue(o, out oldPos))
                {
                    int thisX = (int)(o.position.X / 60f);
                    int thisY = (int)(o.position.Z / 60f);

                    // Remove from old location
                    SortedList<float, GameObject> l = m_grid[oldPos.X, oldPos.Y];
                    l.RemoveAt(l.IndexOfValue(o));

                    // Make sure the object hasn't moved off the edge of the world
                    int maxX = m_grid.GetLength(0);
                    int maxY = m_grid.GetLength(1);
                    if (thisX >= 0 && thisX < maxX && thisY >= 0 && thisY < maxY)
                    {
                        // if not, put in new location
                        m_grid[thisX, thisY].Add(o.position.Y, o);
                        m_index[o] = new Point(thisX, thisY);
                    }
                    else
                    {
                        // Otherwise get rid of it completely
                        m_index.Remove(o);
                    }

                }
            }

            // Run AI on npcs
            foreach (AIObject npc in m_game.allAI)
                npc.doAI(this);

            base.Update(gameTime);
        }

        #endregion
    }
}