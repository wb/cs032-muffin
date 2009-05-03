using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Muffin
{
    public class GraphEdge<T>
    {
        private double m_weight;

        public GraphEdge(T destination, bool isPassable)
        { // Should typically only be called with isPassable = false, pass int weight when isPassable should be true
            node = destination;
            passable = isPassable;
        }

        public GraphEdge(T destination, double edgeWeight) 
        {
            node = destination;
            weight = edgeWeight;
            passable = true;
        }

        public T node { get; set; }
        public double weight 
        {
            get 
            { 
                if (passable) 
                    return m_weight; 
                else 
                    return -1;
            } 
            set 
            { 
                m_weight = value; 
                if (value > 0) 
                    passable = true; 
                else 
                    passable = false; 
            } 
        }

        public bool passable { get; set; }
    }

    public class WeightedGraph<T>
    {
        private Dictionary<T, List<GraphEdge<T>>> m_data;

        public WeightedGraph() 
        {
            m_data = new Dictionary<T, List<GraphEdge<T>>>();
        }

        public void AddNode(T node) 
        {
            // Make sure that node isn't in the graph yet
            if (!m_data.ContainsKey(node))
            {
                m_data.Add(node, new List<GraphEdge<T>>());
            }
        }

        public List<GraphEdge<T>> GetEdges(T node)
        {
            List<GraphEdge<T>> edges = null;
            if (m_data.TryGetValue(node, out edges))
                return edges;
            else
                return null;
        }

        public void SetEdge(T from, T to, double weight)
        {
            if (!m_data.ContainsKey(from))
                AddNode(from);
            if (!m_data.ContainsKey(to))
                AddNode(to);

            List<GraphEdge<T>> edges;
            if (m_data.TryGetValue(from, out edges))
            {
                bool found = false;
                foreach (GraphEdge<T> e in edges)
                {
                    if (e.node.Equals(to))
                    {
                        found = true;
                        e.weight = weight;
                    }

                    if (found)
                        break;
                }
            }
        }


    }
}
