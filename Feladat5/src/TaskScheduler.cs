using System;
using System.Collections.Generic;
using System.Threading;

namespace Feladat5 {
    
    public class TaskScheduler {

        private Action<Task> onTaskFinishedHandler;
        
        private Object lockObj = new Object();
        //private HashSet<int> runningColors;
        private UniqueQueue<int> idleColors = new UniqueQueue<int>(); // HashSet ?
        private Dictionary<int, Queue<Task>> idleTasksQueueByColor = new Dictionary<int, Queue<Task>>();
        private Dictionary<int, Task> runningTasksByColor = new Dictionary<int, Task>();
        private List<TaskConsumer> consumers = new List<TaskConsumer>();

        public TaskScheduler(Action<Task> onTaskFinishedHandler) {
            this.onTaskFinishedHandler = onTaskFinishedHandler;
        }

        public void StartConsumerThreads(int n) {
            for (int i = 0; i < n; i++) {
                var consumer = new TaskConsumer(this);
                consumers.Add(consumer);
                consumer.Start();
            }
        }

        public void AddTask(Task task) {
            lock (lockObj) {
                var color = task.Color;
                if (!idleTasksQueueByColor.TryGetValue(color, out var queue)) {
                    queue = new Queue<Task>();
                    idleTasksQueueByColor[color] = queue;
                }
                queue.Enqueue(task);
                idleColors.Enqueue(color);
                Monitor.Pulse(lockObj); // TODO PulseAll / Pulse ???
            }
        }
        
        
        // Elkéri a következő futtatható task-ot.
        // Ha épp nem áll rendelkezésre, akkor vár, amíg nem lesz
        private Task GetNextTask() {
            //bool haveToWaitForTask = true;
            while (true) {
                lock (lockObj) {
                    if (idleColors.Count > 0) {
                        var theColor = idleColors.Dequeue();
                        bool found = idleTasksQueueByColor.TryGetValue(theColor, out var idleTasksWithTheColor);
                        if (found && idleTasksWithTheColor.Count > 0) {
                            var task = idleTasksWithTheColor.Dequeue();
                            runningTasksByColor.Add(theColor, task);
                            return task;
                        } else Console.WriteLine("ez nem jó :(");
                    }
                    Monitor.Wait(lockObj);
                }
            }
        }


        private void OnTaskFinished(Task task) {
            removeTask(task);
            lock (lockObj) {
                Monitor.Pulse(lockObj); // TODO PulseAll / Pulse ???
            }
            onTaskFinishedHandler(task);
        }

        private void OnTaskFailed(Task task) {
            Console.WriteLine("OnTaskFailed !!!!");
            if (task == null) {
                Console.WriteLine("OnTaskFailed(null) !!!!");
                return;
            }
            removeTask(task);
            // TODO AddTask(task);
        }
        
        /*
        private Queue<int> idleColors; // HashSet ?
        private Dictionary<int, Queue<Task>> idleTasks;
        private Dictionary<int, Task> runningTasksByColor;
        */

        private void removeTask(Task task) {
            lock (lockObj) {
                var color = task.Color;
                runningTasksByColor.Remove(color);                
                if (idleTasksQueueByColor.TryGetValue(color, out var queue)) {
                    if (queue.Count > 0)
                        idleColors.Enqueue(color);
                    else
                        idleTasksQueueByColor[color] = null;
                }
                
            }
        }
        
        
        public class TaskConsumer {
            
            private TaskScheduler scheduler;
            private Task task;
            private Thread thread;

            private bool running = false;
            
            public TaskConsumer(TaskScheduler scheduler) {
                this.scheduler = scheduler;
                thread = new Thread(new ThreadStart(run));
            }

            void run() {
                Task task = null;
                while (running) {
                    //try { // TODO uncomment try-catch
                        task = scheduler.GetNextTask();
                        task.run();
                        scheduler.OnTaskFinished(task);
                    /*}
                    catch(Exception e) {
                        Console.WriteLine(e.Message);
                        scheduler.OnTaskFailed(task);
                        throw e;
                    }*/
                }
            }

            public void Stop() {
                running = false;
            }

            public void Start() {
                if (!running) {
                    running = true;
                    thread.Start();
                }
            }
            
        }


    }
    
}