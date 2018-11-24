using System.Collections.Generic;

namespace Feladat5 {
    
    public class UniqueQueue<T> {

        private HashSet<T> hashSet;
        private LinkedQueue<T> queue;

        public UniqueQueue() {
            hashSet = new HashSet<T>();
            queue = new LinkedQueue<T>();
        }

        public int Count => hashSet.Count;

        public void Clear() {
            hashSet.Clear();
            queue.Clear();
        }

        public bool Contains(T item) => hashSet.Contains(item);

        public void Enqueue(T item) {
            if (hashSet.Add(item))
                queue.Enqueue(item);
        }

        public T Dequeue() {
            T item = queue.Dequeue();
            hashSet.Remove(item);
            return item;
        }

        public void Remove(T item) {
            bool found = hashSet.Remove(item);
            if (found) {
                // TODO remove from queue
                queue.Remove(item);
            }
        }

        public T Peek() => queue.Peek();

        //public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => queue.GetEnumerator();
        
    }
}