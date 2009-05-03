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
    class Path<Node> : IEnumerable<Node>
    {
        public Node LastStep { get; private set; }
        public Path<Node> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }

        private Path(Node lastStep, Path<Node> previousSteps, double totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(Node start) : this(start, null, 0) { }

        public Path<Node> AddStep(Node step, double stepCost)
        {
            return new Path<Node>(step, this, TotalCost + stepCost);
        }

        public IEnumerator<Node> GetEnumerator()
        {
            for (Path<Node> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    public class AI : Microsoft.Xna.Framework.GameComponent
    {
        private class Neighbor
        {
            public double distance { get; set; }
            public TerrainObject tile { get; set; }
        }

        private MuffinGame m_game;
        //private WeightedGraph<TerrainObject> m_world;
        private SortedList<float, GameObject>[,] m_grid;
        private int m_maxX;
        private int m_maxY;

        private Dictionary<GameObject, Point> m_index;

        public MuffinGame game { get { return m_game; } }

        #region General External Methods

        public GameObject topmostObject(float X, float Y)
        {
            int gridX = (int)Math.Round((X / 60f));
            int gridY = (int)Math.Round((Y / 60f));

            return topmostObjectGrid(gridX, gridY);
        }

        public TerrainObject topmostTerrain(float X, float Y)
        {
            int gridX = (int)Math.Round((X / 60f));
            int gridY = (int)Math.Round((Y / 60f));

            return topmostTerrainGrid(gridX, gridY);

        }

        #endregion

        #region Internal Methods

        public AI(Game game)
            : base(game)
        {
            m_game = (MuffinGame)game;
            //m_world = new WeightedGraph<TerrainObject>();
            m_index = new Dictionary<GameObject, Point>();
        }

        private GameObject topmostObjectGrid(int X, int Y)
        {
            if (X < 0 || X >= m_maxX || Y < 0 || Y >= m_maxY)
                return null;

            if (m_grid[X, Y] != null)
            {
                return m_grid[X, Y].Values[m_grid[X, Y].Count - 1];
            }
            else
                return null;
        }

        private TerrainObject topmostTerrainGrid(int X, int Y)
        {
            if (X < 0 || X >= m_maxX || Y < 0 || Y >= m_maxY)
                return null;

            GameObject o = topmostObjectGrid(X, Y);
            if (o is TerrainObject)
            {
                return (TerrainObject)o;
            }
            else
                {
                    if (m_grid[X, Y] == null)
                        return null;

                    int idx = m_grid[X,Y].Count - 1;
                    if (idx > 0)
                    {
                        o = m_grid[X, Y].Values[idx];
                        while (!(o is TerrainObject) && (idx > 0))
                            o = m_grid[X, Y].Values[--idx];

                        if (!(o is TerrainObject))
                            return null;
                        else
                            return (TerrainObject)o;
                    }
                    else
                        return null;
                }
        }

        public void buildGrid()
        {
            m_maxX = 0; m_maxY = 0;

            // First, build the grid
            foreach (TerrainObject t in m_game.allTerrain)
            {
                int gridX = (int)Math.Round(t.position.X / 60f);
                int gridY = (int)Math.Round(t.position.Z / 60f);

                if (gridX > m_maxX)
                    m_maxX = gridX;
                if (gridY > m_maxY)
                    m_maxY = gridY;
            }

            m_maxX++; m_maxY++;
            m_grid = new SortedList<float, GameObject>[m_maxX, m_maxY];
            foreach (GameObject o in m_game.allObjects)
            {
                // Add all objects except AIObjects and PlayerObjects and CollectableObjects to the grid
                if (!(o is AIObject || o is PlayerObject || o is CollectableObject))
                {
                    int gridX, gridY;
                    if (o is TerrainObject)
                    {
                        gridX = (int)Math.Round(o.position.X / 60f);
                        gridY = (int)Math.Round(o.position.Z / 60f);
                    }
                    else
                    {
                        gridX = (int)Math.Round(o.position.X / 60.0f);
                        gridY = (int)Math.Round(o.position.Z / 60.0f);
                    }

                    if (m_grid[gridX, gridY] == null)
                        m_grid[gridX, gridY] = new SortedList<float, GameObject>();

                    m_grid[gridX, gridY].Add(o.position.Y, o);
                    m_index.Add(o, new Point(gridX, gridY));
                }
            }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            buildGrid();

            // Then, build the graph from the grid
/*            for (int x = 0; x < m_maxX; x++)
            {
                for (int y = 0; y < m_maxY; y++)
                {
                    TerrainObject o1 = topmostTerrainGrid(x, y);
                    if(o1 != null)
                        for(int x2 = Math.Max(x-1,0); x2 < Math.Min(x + 2, m_maxX); x2++)
                            for (int y2 = Math.Max(y - 1, 0); y2 < Math.Min(y + 2, m_maxY); y2++)
                            {
                                if (x2 == x && y2 == y)
                                    continue;

                                TerrainObject o2 = topmostTerrainGrid(x2, y2);
                                if (o2 != null)
                                {
                                    // Make sure this edge is valid
                                    // Right now the AI can't jump, so any sudden upward gradient is impassable
                                    // Falling down is ok, to a point...
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
*/
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
                    if(l != null)
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
                    int thisX = (int)Math.Round(o.position.X / 60f);
                    int thisY = (int)Math.Round(o.position.Z / 60f);

                    // Remove from old location
                    SortedList<float, GameObject> l = m_grid[oldPos.X, oldPos.Y];
                    try
                    {
                        l.RemoveAt(l.IndexOfValue(o));
                    }
                    catch (Exception e) { }

                    // Make sure the object hasn't moved off the edge of the world
                    int maxX = m_grid.GetLength(0);
                    int maxY = m_grid.GetLength(1);
                    if (thisX >= 0 && thisX < maxX && thisY >= 0 && thisY < maxY)
                    {
                        // if not, put in new location
                        try
                        {
                            m_grid[thisX, thisY].Add(o.position.Y, o);
                        }
                        catch (Exception e) { }
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

        private List<Neighbor> getNeighbors(TerrainObject o)
        {
            List<Neighbor> n = new List<Neighbor>();

            int gridX = (int)Math.Round(o.position.X / 60f);
            int gridY = (int)Math.Round(o.position.Z / 60f);
            int sx = Math.Max(0, gridX - 1);
            int ex = Math.Min(m_maxX, gridX + 1);
            int sy = Math.Max(0, gridY - 1);
            int ey = Math.Min(m_maxY, gridY + 1);
            for (int x = sx; x <= ex; x++)
            {
                for (int y = sy; y <= ey; y++)
                {
                    if (x == gridX && y == gridY)
                        continue;

                    GameObject c = topmostObjectGrid(x, y);
                    TerrainObject t = topmostTerrainGrid(x, y);
                    if (c != null && (c is TerrainObject || c is CollectableObject))
                    {
                        if ((t.position.Y <= o.position.Y) && (Math.Abs(t.position.Y - o.position.Y) <= GameConstants.MaxFallDistance))
                        {
                            Neighbor h = new Neighbor();

                            h.tile = t;
                            h.distance = (t.position - o.position).Length();

                            n.Add(h);
                        }
                    }
//                    else
//                    {
//                        Console.Out.WriteLine("AI Excluding blocked path...");
//                    }
                }
            }


            return n;
        }

        private double directDist(TerrainObject start, TerrainObject end)
        {
            if (end == null || start == null)
                return 0;

            return (end.position - start.position).Length();
        }

        public List<Vector3> findPath(TerrainObject start, TerrainObject end)
        {
            DateTime _start = DateTime.Now;
            List<Vector3> pl = new List<Vector3>();
            if (start == null)
                return pl;

            HashSet<TerrainObject> closed = new HashSet<TerrainObject>();
            PriorityQueue<double, Path<TerrainObject>> pq = new PriorityQueue<double, Path<TerrainObject>>();
            pq.Enqueue(0, new Path<TerrainObject>(start));

            TimeSpan _elapsed = new TimeSpan(0);
            while (!pq.IsEmpty)
            {
                _elapsed = DateTime.Now - _start;

                Path<TerrainObject> path = pq.Dequeue();
                if (closed.Contains(path.LastStep))
                    continue;
                // If pathfinding is taking too long, just use the best path so far
                // it should get us going in the right general direction
                if (_elapsed.TotalMilliseconds > 50.0 || path.LastStep.Equals(end))
                {
                    foreach (TerrainObject o in path)
                        pl.Insert(0, new Vector3(o.position.X, o.position.Y, o.position.Z));

                    if(pl.Count > 1)
                        pl.RemoveAt(0);

                    return pl;
                }
                closed.Add(path.LastStep);
                foreach (Neighbor n in getNeighbors(path.LastStep))
                {
                    Path<TerrainObject> newpath = path.AddStep(n.tile, n.distance);
                    pq.Enqueue(n.distance + directDist(n.tile, end), newpath);
                }
            }

            return pl;
        }

        #endregion
    }
}