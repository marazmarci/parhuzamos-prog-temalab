using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;


namespace Feladat5 {

    [TestFixture]
    public class UnitTests {

        private const int REPEATS = 50;

        private static int BLUE = 1;
        private static int GREEN = 2;
        private static int RED = 3;
        
        private static bool printLog = false;
        
        private static List<Task> exampleTasks = new List<Task>{
            new RandomAvgTask(BLUE,  10000),
            new RandomAvgTask(BLUE,  10000),
            new RandomAvgTask(GREEN, 10000),
            new RandomAvgTask(RED,   10000),
            new RandomAvgTask(BLUE,  10000)
        };


        private TaskScheduler scheduler;
        private List<TaskLogEntry> log;


        /*
         *  A feladatban leírt példa 2 fogyasztóval, a feladatokat egyből beadva.
         */
        [Test]
        public void TheExampleTasks1() {
            int nConsumerThreads = 2;
            for (int repeat = 1; repeat <= REPEATS; repeat++) {
                Assert.True(RunTasksAndValidateSchedule(nConsumerThreads, exampleTasks));
            }
        }

        
        /*
         *  A feladatban leírt példa 1-10 számú fogyasztóval, a feladatokat egyből beadva.
         */
        [Test]
        public void TheExampleTasks2() {
            for (int repeat = 1; repeat <= REPEATS; repeat++)
            for (int nConsumerThreads = 1; nConsumerThreads <= 10; nConsumerThreads++)
                Assert.True(RunTasksAndValidateSchedule(nConsumerThreads, exampleTasks));
        }

        
        /*
         *  
         */
        [Test]
        public void RandomTasks() {
            var rand = new Random(42);
            int nTasks = 100;
            int nColors = 5;
            for (int nConsumerThreads = 1; nConsumerThreads <= 100; nConsumerThreads++) {
                ResetSchedulerAndLog();
                scheduler.StartConsumerThreads(nConsumerThreads);
                for (int t = 0; t < nTasks; t++) {
                    int taskComplexity = rand.Next(10_000, 100_000);
                    int color = rand.Next(0, nColors);
                    scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
                }
                scheduler.JoinAndStopConsumerThreads();
                bool valid = ValidateSchedule(log);
                Assert.True(valid);
            }
        }




        private double RandomThroughputTest(int nConsumerThreads, int nColors, int nTasks, int minTaskComplexity = 10_000, int maxTaskComplexity = 100_000) {
            var rand = new Random(42);
            const int repeats = 10;
            // TODO time measurement
            for (int repeat = 1; repeat <= repeats; repeat++) {
                ResetSchedulerAndLog();
                scheduler.StartConsumerThreads(nConsumerThreads);
                for (int t = 0; t < nTasks; t++) {
                    int taskComplexity = rand.Next(minTaskComplexity, maxTaskComplexity);
                    int color = rand.Next(0, nColors);
                    scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
                }
                scheduler.JoinAndStopConsumerThreads();
                bool valid = ValidateSchedule(log);
                Assert.True(valid);
                Debug.Assert(valid);
            }
            return 0.0;
        }
        
        
        
        
        [SetUp]
        public void InitTest() {
            SetThreadName();
            ResetSchedulerAndLog();
            //scheduler.DebugMode = true;
        }


        private void ResetSchedulerAndLog() {
            log = new List<TaskLogEntry>();
            scheduler = new TaskScheduler(
                (task) => AddToLog(log, task, TaskLogEntryType.ADD),
                (task) => AddToLog(log, task, TaskLogEntryType.START),
                (task) => AddToLog(log, task, TaskLogEntryType.FINISH),
                (task, e) => { AddToLog(log, task, TaskLogEntryType.FAIL); Console.Error.WriteLine(e.StackTrace);
            });
        }
        

        public static void Main(string[] args) {
            SetThreadName();
            var tests = new UnitTests();
            tests.InitTest();
            tests.scheduler.DebugMode = true;
            tests.TheExampleTasks1();
            //tests.RandomThroughputTest(10, 5, 1000);
            //bool valid = tests.RunTasksAndValidateSchedule(2, exampleTasks);
            //Console.WriteLine("valid = " + valid);
        }
        
        

        private bool RunTasksAndValidateSchedule(int nConsumerThreads, List<Task> tasks) {
            
            ResetSchedulerAndLog();
            
            scheduler.StartConsumerThreads(nConsumerThreads);
            tasks.ForEach(scheduler.AddTask);
            
            //Console.WriteLine("tasks added!");

            scheduler.JoinAndStopConsumerThreads();
            //Console.WriteLine("tasks finished!");

            return ValidateSchedule(log);
        }


        private static void AddToLog(List<TaskLogEntry> log, Task task, TaskLogEntryType type, Exception e = null) {
            var entry = new TaskLogEntry(task, type, Thread.CurrentThread, e);
            if (printLog) {
                Console.WriteLine(entry);
                if (type == TaskLogEntryType.FAIL)
                    Console.Error.WriteLine("^^^ FAILED!!!!!");
            }
            log.Add(entry);
        }


        private static bool ValidateSchedule(List<TaskLogEntry> log) {

            //bool valid = true;
            var colors = new HashSet<int>();
            var runningColors = new HashSet<int>();
            var taskToThreadMap = new Dictionary<Task,Thread>();
            
            bool rule1ok = true;
            bool rule3ok = true;
            bool rule4ok = true;
            

            foreach (var entry in log) {

                var color = entry.Color;
                var type = entry.Type;
                var task = entry.Task;
                var thread = entry.Thread;

                colors.Add(color);

                
                // CHECK RULE #1:
                
                const string rule1 = "Egy feladatot csak egy végrehajtónak szabad kiadni.";

                switch (type) {
                    case TaskLogEntryType.START:
                        if (taskToThreadMap.TryGetValue(task, out var _)) {
                            rule1ok = false;
                            PrintBadSchedule(entry, rule1);
                        } else {
                            taskToThreadMap.Add(task, thread);
                        }
                        break;
                    case TaskLogEntryType.FINISH:
                        if (taskToThreadMap.TryGetValue(task, out var starterThread)) {
                            var finisherThread = thread;
                            if (starterThread != finisherThread) {
                                rule1ok = false;
                                PrintBadSchedule(entry, rule1);
                            }
                        } else {
                            rule1ok = false;
                            PrintBadSchedule(entry, "task finished without started!!?!");
                        }
                        break;
                }




                // CHECK RULE #3:
                
                const string rule3 = "Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.";

                if (runningColors.Contains(color)) {
                    switch (type) {
                        case TaskLogEntryType.START:
                            // not ok
                            rule3ok = false;
                            PrintBadSchedule(entry, rule3);
                            break;
                        case TaskLogEntryType.FINISH:
                            // ok
                            runningColors.Remove(color);
                            break;
                    }
                } else {
                    switch (type) {
                        case TaskLogEntryType.START:
                            // ok
                            runningColors.Add(color);
                            break;
                        case TaskLogEntryType.FINISH:
                            // not ok
                            rule3ok = false;
                            PrintBadSchedule(entry, rule3);
                            break;
                    }
                }

            }

            
            // CHECK RULE #4:

            const string rule4 = "Egy adott színhez tartozó feladatokat a beadás sorrendjében kell végrehajtani.";

            foreach (var color in colors) {

                int lastFinishId = -1;
                int lastStartId = -1;
                foreach (var entry in log) {
                    if (entry.Color == color) {

                        var id = entry.ID;

                        switch (entry.Type) {
                            case TaskLogEntryType.START:
                                if (id <= lastStartId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                lastStartId = id;
                                break;
                            case TaskLogEntryType.FINISH:
                                if (id <= lastFinishId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                lastFinishId = id;
                                break;
                        }


                    }
                }

            }
            
            return rule1ok && rule3ok && rule4ok;
        }

        private static void PrintBadSchedule(TaskLogEntry entry, string brokenRule) {
            Console.WriteLine($"Bad schedule: {entry}  \"{brokenRule}\"");
        }

        /* Fő szál elnevezése a könnyebb debugging érdekében */
        private static void SetThreadName() {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "MainThread";
        }


    }


}