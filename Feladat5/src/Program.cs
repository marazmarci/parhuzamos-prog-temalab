using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using static TaskLogEntryType;

namespace Feladat5 {
    
    public class Program {

        private const int nColors = 10;
        private static int nConsumerThreads;
        private static int runSeconds = 10;
            
        private static volatile bool producerRunning = true;

        
        private static List<TaskLogEntry> log = new List<TaskLogEntry>();
        private static TaskScheduler scheduler = new TaskScheduler(
            (task) => { AddToLog(task, ADD); },
            (task) => { AddToLog(task, START); },
            (task) => { AddToLog(task, FINISH); },
            (task,e) => { AddToLog(task, FAIL, e); }
        );
        

        public static void Main() {

            Console.Write("How many consumer threads do you want? ");
            string input = Console.ReadLine();
            Console.WriteLine();
            if (!Int32.TryParse(input, out nConsumerThreads))
                nConsumerThreads = -1;
            if (nConsumerThreads < 1) {
                Console.WriteLine("Your input was invalid, going with 10x threads");
                nConsumerThreads = 10;
            }
            

            Console.WriteLine($"Starting {nConsumerThreads}x consumer threads...");
            scheduler.StartConsumerThreads(nConsumerThreads);

            Console.WriteLine("Starting 1x producer thread...");
            var producer = new Thread(Producer);
            producer.Name = "ProducerThread";
            producer.Start();

            Console.WriteLine($"Let the scheduler run for {runSeconds} seconds, while feeding it with random tasks...");
            Console.WriteLine($"(with random colors out of {nColors}x)");
            Thread.Sleep(runSeconds * 1000);
            Console.WriteLine("Stopping the scheduler...");
            producerRunning = false;
            scheduler.AbortAndJoinConsumerThreads();

            int finishedTaskCount = scheduler.FinishedTaskCount;
            double tasksPerSec = (double) finishedTaskCount / runSeconds;
            Console.WriteLine($"The scheduler has eaten {finishedTaskCount}x tasks! ({tasksPerSec} tasks/sec)");
            Console.WriteLine("Checking if the scheduler has scheduled according to the rules...");
            bool valid = ScheduleValidator.IsValid(log);
            if (valid)
                Console.WriteLine("The schedule was valid! :)");
            else
                Console.WriteLine("The schedule was bad!!!! :(");

        }


        private static void Producer() {
            var rand = new Random(42);
            while (producerRunning) {
                int loops = rand.Next(10);
                int sleep = rand.Next(0, 10);
                for (int i = 0; i < loops; i++) {
                    int taskComplexity = rand.Next(100_000, 1_000_000);
                    int color = rand.Next(nColors);
                    scheduler.AddTask(new RandomAvgTask(color, taskComplexity));
                }
                Thread.Sleep(sleep);
            }
            
        }
        
        
        private static void AddToLog(Task task, TaskLogEntryType type, Exception e = null)
            => log.Add(new TaskLogEntry(task, type, Thread.CurrentThread, e));


    }
    
}