using System.Collections.Generic;
using NUnit.Framework;

namespace Feladat5 {
    
    [TestFixture]
    public class UniqueQueueTest {

        private UniqueQueue<int> queue;

        [SetUp]
        public void InitTest() {
            queue = new UniqueQueue<int>();
        }

        [Test]
        public void Test1() {
            queue.EnqueueIfNotContainsAlready(1);
            var item = queue.Dequeue();
            Assert.AreEqual(item, 1);
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test2() {
            
            // enqueue 1,2...99,100
            for (int i = 1; i <= 100; i++) {
                queue.EnqueueIfNotContainsAlready(i);
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
        public void Test3() {
            queue.EnqueueIfNotContainsAlready(1);
            queue.EnqueueIfNotContainsAlready(1);
            var item = queue.Dequeue();
            AssertQueueIsEmpty();
        }

        [Test]
        public void Test4() {
            
            // enqueue 1,2...99,100
            for (int i = 1; i <= 100; i++) {
                queue.EnqueueIfNotContainsAlready(i);
            }
            
            Assert.AreEqual(queue.Count, 100);
            
            queue.EnqueueIfNotContainsAlready(1);
            
            Assert.AreEqual(queue.Count, 100);
            
            // enqueue 1,2...99,100 again
            for (int i = 1; i <= 100; i++) {
                queue.EnqueueIfNotContainsAlready(i);
                Assert.AreEqual(queue.Count, 100);
            }
            
            // dequeue 1,2...99,100
            for (int i = 1; i <= 100; i++) {
                var item = queue.Dequeue();
                Assert.AreEqual(item, i);
            }
            
            AssertQueueIsEmpty();
        }


        private void AssertQueueIsEmpty() {
            Assert.AreEqual(queue.Count, 0);
            Assert.True(queue.IsEmpty());
        }


    }
    
}