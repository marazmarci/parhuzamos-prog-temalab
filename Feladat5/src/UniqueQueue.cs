using System.Collections.Generic;

namespace Feladat5 {
    
    public class UniqueQueue<T> {

        private readonly HashSet<T> hashSet;
        private readonly Queue<T> queue;

        public UniqueQueue() {
            hashSet = new HashSet<T>();
            queue = new Queue<T>();
        }

        public int Count => hashSet.Count;

        public void Clear() {
            hashSet.Clear();
            queue.Clear();
        }

        public bool Contains(T item) => hashSet.Contains(item);

        public void EnqueueIfNotContainsAlready(T item) {
            bool added = hashSet.Add(item);
            if (added)
                queue.Enqueue(item);
        }

        public T Dequeue() {
            T item = queue.Dequeue();
            hashSet.Remove(item);
            return item;
        }

        /*
        public bool Remove(T item) {
            bool found = hashSet.Remove(item);
            if (found) {
                count--;
                queue.Remove(item);
            }
            return found;
        }
        */

        public T Peek() => queue.Peek();

        public bool IsEmpty() => Count == 0;

    }
}