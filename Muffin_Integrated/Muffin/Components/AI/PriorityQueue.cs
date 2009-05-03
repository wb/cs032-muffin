using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Muffin.Components.AI
{
    class PriorityQueue<P, V>
    {
        // A sorted dictionary of queues
        // Allows more than one item to have the same priority easily
        private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();

        public void Enqueue(P priority, V value)
        {
            Queue<V> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new Queue<V>();
                list.Add(priority, q);
            }
            q.Enqueue(value);
        }

        public V Dequeue()
        {
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) 
                list.Remove(pair.Key);
            return v;
        }

        public bool IsEmpty
        {
            get { return !list.Any(); }
        }
    }

}
