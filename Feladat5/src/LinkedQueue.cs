using System;
using System.Collections;
using System.Collections.Generic;

namespace Feladat5 {
    
    internal class Node<T> {
        public T Data { get; set; }
        public Node<T> Next { get; set; }
        public Node(T data) {
            this.Data = data;
        }
    }
    
    public class LinkedQueue<T> {
        
        internal Node<T> head;
        internal Node<T> tail;
        private int count = 0;
        
        public LinkedQueue() { }
        
        public void Enqueue(T data) {
            var newNode = new Node<T>(data);
            if (head == null) {
                head = newNode;
                tail = head;
            } else {
                tail.Next = newNode;
                tail = tail.Next;
            }
            count++;
        }
        
        public T Dequeue() {
            var result = Peek();
            head = head.Next;
            return result;
        }

        public T Peek() {
            if (head == null)
                throw new Exception("Queue is Empty");
            return head.Data;
        }

        public bool Remove(T item) {
            Node<T> prev = null;
            for (var iter = head; iter != null; iter = iter.Next) {
                if (iter.Data.Equals(item)) {
                    if (prev != null)
                        prev.Next = iter.Next;
                    return true;
                }
                prev = iter;
            }
            return false;
        }

        public void Clear() {
            count = 0;
            head = tail = null;
        }
        
        public int Count => this.count;
        
    }
    
}