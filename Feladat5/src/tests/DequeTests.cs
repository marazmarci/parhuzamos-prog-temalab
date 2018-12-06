using System.Collections.Generic;
using NUnit.Framework;

namespace Feladat5 {
    
    [TestFixture]
    public class DequeTests {

        private Deque<int> queue;

        [SetUp]
        public void InitTest() => queue = new Deque<int>();

        [Test]
        public void Test1() {
            queue.EnqueueHead(1);
            var item = queue.Dequeue();
            Assert.AreEqual(item, 1);
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test2() {
            queue.EnqueueTail(1);
            var item = queue.Dequeue();
            Assert.AreEqual(item, 1);
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test3() {
            queue.EnqueueTail(2);
            queue.EnqueueHead(1);
            var item1 = queue.Dequeue();
            Assert.AreEqual(item1, 1);
            var item2 = queue.Dequeue();
            Assert.AreEqual(item2, 2);
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test4() {
            
            // enqueue 1,2...99,100
            for (int i = 1; i <= 100; i++) {
                queue.EnqueueTail(i);
            }
            
            Assert.AreEqual(queue.Count, 100);
            
            // dequeue 1,2...99,100
            for (int i = 1; i <= 100; i++) {
                int countBeforePeek = queue.Count;
                var peeked = queue.Peek();
                int countAfterPeek = queue.Count;
                var dequeued = queue.Dequeue();
                Assert.AreEqual(countBeforePeek, countAfterPeek);
                Assert.AreEqual(dequeued, peeked);
                Assert.AreEqual(dequeued, i);
            }
            
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test5() {
            
            for (int i = 50; i >= 1; i--)
                queue.EnqueueHead(i);
            
            for (int i = 51; i <= 100; i++)
                queue.EnqueueTail(i);

            Assert.AreEqual(queue.Count, 100);
            
            for (int i = 1; i <= 100; i++) {
                var dequeued = queue.Dequeue();
                Assert.AreEqual(dequeued, i);
            }
            
            AssertQueueIsEmpty();
        }


        private void AssertQueueIsEmpty() => Assert.AreEqual(queue.Count, 0);


    }
    
}