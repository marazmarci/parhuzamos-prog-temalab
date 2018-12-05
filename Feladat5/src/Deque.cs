using System;
using System.Collections.Generic;

namespace Feladat5 {
    
    public class Deque<T> {
        
        private LinkedList<T> list = new LinkedList<T>();
        
        public int Count => list.Count;
        
        
        public void EnqueueTail(T item) => list.AddLast(item);

        public void EnqueueHead(T item) => list.AddFirst(item);

        public T Dequeue() {
            var result = Peek();
            list.RemoveFirst();
            return result;
        }

        public T Peek() => list.First.Value;

        public void Clear() => list.Clear();

        
    }
    
}