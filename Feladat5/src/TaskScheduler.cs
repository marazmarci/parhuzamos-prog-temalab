using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace Feladat5 {
    
    
    public class TaskScheduler {

        public bool DebugMode { private get; set; } = false;

        private readonly Action<Task> onTaskAdded;
        private readonly Action<Task> onTaskStarted;
        private readonly Action<Task> onTaskFinished;
        private readonly Action<Task,Exception> onTaskFailed;
        
        private readonly Object lockObj = new Object();
        private readonly Object joinLockObj = new Object();
        private readonly Object outsideWorldLock = new Object();
        
        private readonly UniqueQueue<int> idleColors = new UniqueQueue<int>();
        private readonly Dictionary<int, Deque<Task>> colorToIdleTaskDequeMap = new Dictionary<int, Deque<Task>>();
        private readonly HashSet<int> runningColors = new HashSet<int>();
        
        private readonly List<TaskConsumer> consumers = new List<TaskConsumer>();
        
        private int unfinishedTaskCount = 0;
        private int finishedTaskCount = 0;
        public int FinishedTaskCount { get { lock (lockObj) { return finishedTaskCount; } } }

        private int taskIdCounter = 0;
        private bool stopped = true;
        private bool aborted = false;


        public TaskScheduler(Action<Task> onTaskAdded, Action<Task> onTaskStarted, Action<Task> onTaskFinished, Action<Task,Exception> onTaskFailed) {
            this.onTaskAdded = onTaskAdded;
            this.onTaskStarted = onTaskStarted;
            this.onTaskFinished = onTaskFinished;
            this.onTaskFailed = onTaskFailed;
        }
        

        public void StartConsumerThreads(int n) {
            lock (lockObj) {
                Assert(stopped);
                Assert(consumers.Count == 0);
                stopped = false;
                for (int i = 0; i < n; i++) {
                    var consumer = new TaskConsumer(this, i);
                    consumers.Add(consumer);
                    consumer.Start();
                }
            }
        }
        

        public void AddTask(Task task) => AddTask(task, true);
        

        private void AddTask(Task task, bool addToTail) {
            
            Assert(task != null);
            
            lock (lockObj) {
                
                unfinishedTaskCount++;
                
                if (addToTail)
                    task.ID = taskIdCounter++;
                
                var color = task.Color;
                
                if (!colorToIdleTaskDequeMap.TryGetValue(color, out var deque)) {
                    deque = new Deque<Task>();
                    colorToIdleTaskDequeMap[color] = deque;
                }
                
                if (addToTail)
                    deque.EnqueueTail(task);
                else
                    deque.EnqueueHead(task);
                
                if (!runningColors.Contains(color))
                    idleColors.EnqueueIfNotContainsAlready(color);

                if (!addToTail) {
                    lock (outsideWorldLock) {
                        onTaskAdded(task);
                    }
                }
                
                PulseLockObj();                
            }
        }
        
        
        // Elkéri a következő futtatható task-ot.
        // Ha épp nem áll rendelkezésre, akkor addig vár, amíg nem lesz
        // null-t ad vissza, ha le lett kell állnia a fogyasztónak
        private Task GetNextTask() {
            while (true) {
                lock (lockObj) {
                    if (DebugMode) {
                        lock (outsideWorldLock) {
                            Console.WriteLine($"{Thread.CurrentThread.Name} GetNextTask");   
                        }
                    }   
                    if (stopped)
                        return null;
                    if (!idleColors.IsEmpty()) {
                        var theColor = idleColors.Dequeue();
                        Assert(!runningColors.Contains(theColor));
                        bool found = colorToIdleTaskDequeMap.TryGetValue(theColor, out var idleTasksQueueOfTheColor);
                        Assert(found);
                        if (found) {
                            Assert(idleTasksQueueOfTheColor.Count > 0);
                            var task = idleTasksQueueOfTheColor.Dequeue();
                            Assert(task != null);
                            runningColors.Add(theColor);
                            if (DebugMode) {
                                lock (outsideWorldLock) {
                                    Console.WriteLine($"{Thread.CurrentThread.Name} GetNextTask returned {task}");
                                }
                            }
                            Assert(task != null);
                            return task;
                        } else {
                            throw new Exception("task scheduler is in an inconsistent state!");
                        }
                    }
                    WaitLockObj();
                }
            }
        }


        private void OnTaskFinished(Task task) {
            bool pulseJoinLockObj;
            lock (lockObj) {
                AssertColorRunning(task.Color);
                RemoveTask(task);
                finishedTaskCount++;
                lock (outsideWorldLock) {
                    onTaskFinished(task);
                    unfinishedTaskCount--;
                    pulseJoinLockObj = (unfinishedTaskCount == 0);
                }
                PulseLockObj();
            }
            if (pulseJoinLockObj)
                PulseLockObj(useJoinLockObj: true);
        }
        

        private void OnTaskStarted(Task task) {
            AssertColorRunning(task.Color);
            lock (outsideWorldLock) {
                onTaskStarted(task);
            }
        }
        

        private void OnTaskFailed(Task task, Exception exception) {
            lock (lockObj) {
                if (!aborted) {
                    AssertColorRunning(task.Color);
                    RemoveTask(task);
                    lock (outsideWorldLock) {
                        onTaskFailed(task, exception);
                    }
                    AddTask(task, false); // add it back to the head of the deque (not to the tail)
                    unfinishedTaskCount--;
                    //PulseLockObj();
                }
            }
        }
        

        private void AssertColorRunning(int color) {
            if (!DebugMode)
                return;
            bool colorIdle, colorRunning;
            lock (lockObj) {
                colorIdle = idleColors.Contains(color);
                colorRunning = runningColors.Contains(color);
            }
            Assert(!colorIdle);
            Assert(colorRunning);
        }
        

        private void Assert(bool condition) {
            if (DebugMode)
                Debug.Assert(condition);
        }
        

        private void PulseLockObj(bool pulseAll = false, bool useJoinLockObj = false) {
            if (DebugMode) {
                lock (outsideWorldLock) {
                    Console.WriteLine($"{Thread.CurrentThread.Name} pulse" + (pulseAll ? "All" : ""));
                }
            }
            var llock = useJoinLockObj ? joinLockObj : lockObj;
            lock (llock) {
                if (pulseAll)
                    Monitor.PulseAll(llock);
                else
                    Monitor.Pulse(llock);
            }
        }
        

        private void WaitLockObj(bool useJoinLockObj = false) {
            if (DebugMode)
                lock (outsideWorldLock)
                    Console.WriteLine($"{Thread.CurrentThread.Name} waiting" + (useJoinLockObj ? " on joinLockObj" : "") );
            var llock = useJoinLockObj ? joinLockObj : lockObj;
            lock (llock)
                Monitor.Wait(llock);
            if (DebugMode)
                lock (outsideWorldLock)
                    Console.WriteLine($"{Thread.CurrentThread.Name} finished waiting" + (useJoinLockObj ? " on joinLockObj" : "") );
        }

        private void RemoveTask(Task task) {
            lock (lockObj) {
                var theColor = task.Color;
                bool removed = runningColors.Remove(theColor);
                Assert(removed);
                Assert(!idleColors.Contains(theColor));
                bool ok = colorToIdleTaskDequeMap.TryGetValue(theColor, out var idleTaskQueueOfTheColor);
                Assert(ok);
                if (ok && idleTaskQueueOfTheColor != null) {
                    if (idleTaskQueueOfTheColor.Count > 0) {
                        idleColors.EnqueueIfNotContainsAlready(theColor);
                    } else {
                        bool removed2 = colorToIdleTaskDequeMap.Remove(theColor);
                        Assert(removed2);
                    }                    
                }
            }
        }

        private void Join(bool useJoinLockObj = true) {
            Assert(!stopped);
            lock (joinLockObj) {
                lock (lockObj) {
                    Assert(unfinishedTaskCount >= 0);
                    if (unfinishedTaskCount == 0)
                        return;
                }
                if (useJoinLockObj)
                    WaitLockObj(useJoinLockObj: true);
            }
        }

        public void JoinAndStopConsumerThreads() {
            Join();
            StopConsumerThreads();
            foreach (var consumer in consumers)
                consumer.Join();
            consumers.Clear();
        }

        private void StopConsumerThreads() {
            Assert(!stopped);
            stopped = true;
            PulseLockObj(pulseAll: true);
        }
        
        public void AbortAndJoinConsumerThreads() {
            lock (lockObj) {
                aborted = true;
                unfinishedTaskCount = 0;
                foreach (var consumer in consumers)
                    consumer.Abort();
            }
            foreach (var consumer in consumers)
                consumer.Join();
            consumers.Clear();
        }


        private class TaskConsumer {
            
            private readonly TaskScheduler scheduler;
            private readonly Thread thread;

            private bool running;
            
            public TaskConsumer(TaskScheduler scheduler, int consumerId) {
                this.scheduler = scheduler;
                thread = new Thread(Run) { Name = $"Consumer#{consumerId}" };
            }

            void Run() {
                Task task = null;
                try {
                    while (running) {
                        try {
                            task = scheduler.GetNextTask();
                            if (task != null) {
                                scheduler.OnTaskStarted(task);
                                task.Run();
                                scheduler.OnTaskFinished(task);
                                task = null;
                            } else {
                                running = false;
                            }
                        } catch(Exception e) {
                            if (e is ThreadAbortException)
                                throw e;
                            scheduler.OnTaskFailed(task, e);
                        }
                    }
                } catch (ThreadAbortException e) {
                    Thread.ResetAbort();
                    if (task != null) {
                        scheduler.OnTaskFailed(task, e);
                    }
                }
                
            }

            public void Start() {
                if (!running) {
                    running = true;
                    thread.Start();
                }
            }

            public void Join() => thread.Join();

            public void Abort() => thread.Abort();
        }

    }
    
}