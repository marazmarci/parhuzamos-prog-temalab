using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;


namespace Feladat5 {

    [TestFixture]
    public class SchedulerTests {

        public static int BLUE = 1;
        public static int GREEN = 2;
        public static int RED = 3;
        
        private static bool printLog = false;
        
        private static List<Task> exampleTasks = new List<Task>{
            new RandomAvgTask(BLUE,  10_000),
            new RandomAvgTask(BLUE,  10_000),
            new RandomAvgTask(GREEN, 20_000),
            new RandomAvgTask(RED,   10_000),
            new RandomAvgTask(BLUE,  10_000)
        };


        private volatile TaskScheduler scheduler;
        private volatile List<TaskLogEntry> log;


        /*
         *  A feladatban leírt példa 2 fogyasztóval, a feladatokat egyből beadva.
         */
        [Test]
        public void TheExampleTasks1() {
            int nConsumers = 2;
            for (int repeat = 1; repeat <= 50; repeat++) {
                Assert.True(RunTasksAndValidateSchedule(nConsumers, exampleTasks));
            }
        }

        
        /*
         *  A feladatban leírt példa 1-10 számú fogyasztóval, a feladatokat egyből beadva.
         */
        [Test]
        public void TheExampleTasks2() {
            for (int repeat = 1; repeat <= 50; repeat++)
            for (int nConsumers = 1; nConsumers <= 10; nConsumers++)
                Assert.True(RunTasksAndValidateSchedule(nConsumers, exampleTasks));
        }

        
        /*
         *  
         */
        [Test]
        public void RandomTasks() {
            var rand = new Random(42);
            int nTasks = 100;
            int nColors = 5;
            for (int nConsumers = 1; nConsumers <= 100; nConsumers++) {
                ResetSchedulerAndLog();
                scheduler.StartConsumerThreads(nConsumers);
                for (int t = 0; t < nTasks; t++) {
                    int taskComplexity = rand.Next(10_000, 100_000);
                    int color = rand.Next(0, nColors);
                    scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
                }
                scheduler.JoinAndStopConsumerThreads();
                Assert.True(ScheduleValidator.IsValid(log));
            }
        }


        [Test]
        public void TestSameColors() {
            int color = 0;
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(10);
            for (int taskComplexity = 10_000; taskComplexity <= 100_000; taskComplexity++)
                scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
            scheduler.JoinAndStopConsumerThreads();
            Assert.True(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule1() {
            // Egy feladatot csak egy végrehajtónak szabad kiadni.
            
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(2);

            var task1 = new RandomAvgTask(BLUE, 10_000);

            for (int i = 0; i < 1000; i++)
                scheduler.AddTask(new RandomAvgTask(0, 10_000));
            
            scheduler.JoinAndStopConsumerThreads();
            
            Assert.True(ScheduleValidator.IsValid(log));
            
        }


        [Test]
        public void TestRule2WhenTaskFails() {
            // Ha a feladat végrehajtása valamilyen okból meghiúsul (pl. a végrehajtó szál elesik), a feladatot vissza kell tenni a végrehajtási sorba.
            
            int nTasks = 100;
            int nColors = 5;
            int nConsumerThreads = 6;
            
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(nConsumerThreads);

            for (int i = 0; i < 100; i++) {
                int color = i % nColors;
                scheduler.AddTask(new RandomAvgTask(color, 10_000));
            }

            scheduler.AddTask(new OnceFailingTask(0, 10_000));
            
            for (int i = 0; i < 100; i++) {
                int color = i % nColors;
                scheduler.AddTask(new RandomAvgTask(color, 10_000));
            }
            
            scheduler.JoinAndStopConsumerThreads();
            
            Assert.True(ScheduleValidator.IsValid(log));
            
        }


        class OnceFailingTask : RandomAvgTask {

            private bool hasFailedOnce = false;

            public OnceFailingTask(int color, int n) : base(color, n) {}

            public override void Run() {
                base.Run();
                if (!hasFailedOnce) {
                    hasFailedOnce = true;
                    throw new Exception("I'm failing intentionally!");
                }
            }
            
        }


        [Test]
        public void TestRule2WhenThreadAborted() {
            // Ha a feladat végrehajtása valamilyen okból meghiúsul (pl. a végrehajtó szál elesik), a feladatot vissza kell tenni a végrehajtási sorba.
            
            int nTasks = 100;
            int nColors = 5;
            int nConsumerThreads = 6;
            
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(nConsumerThreads);

            for (int i = 0; i < 100; i++) {
                int color = i % nColors;
                scheduler.AddTask(new RandomAvgTask(color, 10_000));
            }

            scheduler.AddTask(new OnceThreadAbortingTask(0, 10_000));
            
            for (int i = 0; i < 100; i++) {
                int color = i % nColors;
                scheduler.AddTask(new RandomAvgTask(color, 10_000));
            }
            
            scheduler.JoinAndStopConsumerThreads();
            
            Assert.True(ScheduleValidator.IsValid(log));
            
        }


        class OnceThreadAbortingTask : RandomAvgTask {

            private bool hasAbortedThreadOnce = false;

            public OnceThreadAbortingTask(int color, int n) : base(color, n) {}

            public override void Run() {
                base.Run();
                if (!hasAbortedThreadOnce) {
                    hasAbortedThreadOnce = true;
                    Thread.CurrentThread.Abort();
                }
            }
            
        }


        [Test]
        public void TestRule3() {
            // Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.
            
            int nTasks = 50;
            int nColors = 2;
            int nConsumerThreads = 10;
            
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(nConsumerThreads);

            for (int i = 0; i < 100; i++) {
                int color = i % nColors;
                scheduler.AddTask(new RandomAvgTask(color, 1_000_000));
            }
            
            scheduler.JoinAndStopConsumerThreads();
            
            Assert.True(ScheduleValidator.IsValid(log));
        }
        


        [Test]
        public void TestRule4() {
            // Egy adott színhez tartozó feladatokat a beadás sorrendjében kell végrehajtani.
            
            int nTasks = 50;
            int nColors = 5;
            int nConsumerThreads = 10;
            
            ResetSchedulerAndLog();
            scheduler.StartConsumerThreads(nConsumerThreads);

            for (int i = 0; i < 1000; i++) {
                int color = i % (nColors - 1) + 1;
                scheduler.AddTask(new RandomAvgTask(color, 1_000_000));
                if (i % 10 == 0)
                    scheduler.AddTask(new RandomAvgTask(0, 1_000_000));
            }
            
            scheduler.JoinAndStopConsumerThreads();
            
            Assert.True(ScheduleValidator.IsValid(log));
        }



        private long GetTimestamp() => DateTimeOffset.Now.ToUnixTimeMilliseconds();


        private void RunThroughputTests() {

            int maxConsumerCount = Environment.ProcessorCount * 2;
            int maxColorCount = maxConsumerCount * 2;

            Console.WriteLine("nConsumers;nColors;result");

            int iterations = 0;
            int lastIntPercent = 0;
            long startTime = GetTimestamp();
            
            var testParameters = new List<Tuple<int,int>>();
            for (int nConsumers = 1; nConsumers <= maxConsumerCount; nConsumers++)
                for (int nColors = 1; nColors <= maxColorCount; nColors++)
                    testParameters.Add(new Tuple<int,int>(nConsumers, nColors) );

            new Random(50).Shuffle(testParameters);

            using (var fileWriter = new StreamWriter($"results/throughput.csv")) {
                
                fileWriter.WriteLine("nConsumers;nColors;result");
                
                foreach (var param in testParameters) {
                    int nConsumers = param.Item1;
                    int nColors = param.Item2;
                
                    iterations++;
                    double percent = 100 * (double) iterations / (maxColorCount * maxConsumerCount);
                    
                    double result = ThroughputTest(nConsumers, nColors, repeats: 5, milliseconds: 5000);

                    
                    int intPercent = (int) percent;
                    if (intPercent != lastIntPercent) {
                        long nowTime = GetTimestamp();
                        double elapsedSeconds = (nowTime - startTime) / 1000.0;
                        double remainingPercent = 100 - percent;
                        double speed = percent / elapsedSeconds;
                        double remainingSeconds = remainingPercent / speed;
                        Console.WriteLine($"{intPercent}%");
                        Console.WriteLine($"elapsed: {elapsedSeconds} seconds = {elapsedSeconds / 60} minutes");
                        Console.WriteLine($"remaining: {remainingSeconds} seconds = {remainingSeconds / 60} minutes");
                        Console.WriteLine($"total: {100 / speed} seconds = {100 / speed / 60} minutes");
                        lastIntPercent = intPercent;
                    }
                
                    Console.WriteLine($"{nConsumers};{nColors};{result}");
                    Console.WriteLine();
                    
                    fileWriter.WriteLine($"{nConsumers};{nColors};{result}");
                    fileWriter.Flush();
                
                }
                
            }

            
            
        }


        private double ThroughputTest(int nConsumers, int nColors, int repeats = 10, int milliseconds = 5_000) {
            int minTaskComplexity = 10_000;
            int maxTaskComplexity = 100_000;
            var rand = new Random(42);
            int accumulatedResult = 0;
            // TODO time measurement
            for (int repeat = 1; repeat <= repeats; repeat++) {
                ResetSchedulerAndLog();

                var producer = new Thread(() => {
                    try {
                        while (true) {
                            int taskComplexity = rand.Next(minTaskComplexity, maxTaskComplexity);
                            int color = rand.Next(0, nColors);
                            scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
                        }
                    } catch (ThreadAbortException) { }
                });
                
                producer.Start();
                
                Assert.True(scheduler.FinishedTaskCount == 0);
                scheduler.StartConsumerThreads(nConsumers);

                Thread.Sleep(milliseconds);
                int finishedTasks = scheduler.FinishedTaskCount;
                producer.Abort();
                scheduler.AbortAndJoinConsumerThreads();
                    
                bool valid = ScheduleValidator.IsValid(log);
                if (!valid) {
                    Console.WriteLine();
                    Console.WriteLine("Full schedule:");
                    foreach (var entry in log) {
                        if (entry.Type != TaskLogEntryType.ADD)
                            Console.WriteLine(entry);
                    }
                }
                Assert.True(valid);

                accumulatedResult += finishedTasks;

            }
            return (double) accumulatedResult / repeats;
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
                (task) => {
                    AddToLog(log, task, TaskLogEntryType.ADD);
                },
                (task) => AddToLog(log, task, TaskLogEntryType.START),
                (task) => AddToLog(log, task, TaskLogEntryType.FINISH),
                (task, e) => {
                    AddToLog(log, task, TaskLogEntryType.FAIL);
                    //Console.Error.WriteLine(e.StackTrace);
                }
            );
        }
        

        /*public static void Main(string[] args) {
            SetThreadName();
            var tests = new SchedulerTests();
            tests.InitTest();
            printLog = true;
            tests.scheduler.DebugMode = true;
            //tests.TheExampleTasks1();
            //tests.RandomThroughputTest(10, 5, 1000);
            //tests.TestRule2WhenTaskFails();
            tests.TestRule2WhenThreadAborted();
            ////tests.RunThroughputTests();
            //bool valid = tests.RunTasksAndValidateSchedule(2, exampleTasks);
            //Console.WriteLine("valid = " + valid);
        }*/
        
        

        private bool RunTasksAndValidateSchedule(int nConsumers, List<Task> tasks) {
            
            ResetSchedulerAndLog();
            
            scheduler.StartConsumerThreads(nConsumers);
            tasks.ForEach(scheduler.AddTask);
            
            //Console.WriteLine("tasks added!");

            scheduler.JoinAndStopConsumerThreads();
            //Console.WriteLine("tasks finished!");

            return ScheduleValidator.IsValid(log);
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
    
    // Forrás: https://stackoverflow.com/a/110570/6347943
    static class RandomExtensions {
        public static void Shuffle<T> (this Random rng, List<T> list) {
            int n = list.Count;
            while (n > 1) {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }


}