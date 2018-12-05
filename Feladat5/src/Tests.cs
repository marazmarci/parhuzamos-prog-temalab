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
            new RandomAvgTask(BLUE,  10_000),
            new RandomAvgTask(BLUE,  10_000),
            new RandomAvgTask(GREEN, 20_000),
            new RandomAvgTask(RED,   10_000),
            new RandomAvgTask(BLUE,  10_000)
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
                bool valid = ScheduleValidator.Validate(log);
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
                bool valid = ScheduleValidator.Validate(log);
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

            return ScheduleValidator.Validate(log);
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
        

        /* Fő szál elnevezése a könnyebb debugging érdekében */
        private static void SetThreadName() {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Thread.CurrentThread.Name = "MainThread";
        }


    }


}