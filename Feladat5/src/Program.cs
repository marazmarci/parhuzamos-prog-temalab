using System;
using System.Security.Policy;

namespace Feladat5 {
    
    internal class Program {
        
        public static void Main(string[] args) {
            var scheduler = new TaskScheduler(onTaskFinished);
            scheduler.AddTask(new RandomAvgTask(1,1011));
            scheduler.AddTask(new RandomAvgTask(1,1012));
            scheduler.AddTask(new RandomAvgTask(2,5000));
            scheduler.AddTask(new RandomAvgTask(3,1031));
            scheduler.AddTask(new RandomAvgTask(1,1013));
            Console.WriteLine("tasks added!");
            scheduler.StartConsumerThreads(2);
        }

        private static void onTaskFinished(Task task) {
            Console.WriteLine($"{task} finished.");
        }
        
        
    }
    
    

    class RandomAvgTask : Task {

        private int n;

        public RandomAvgTask(int color, int n) : base(color) {
            this.n = n;
        }

        public override void run() {
            var rand = new Random(42);
            long sum = 0;
            for (int i = 0; i < n; i++)
                sum += rand.Next();
            long avg = sum / n;
            Console.WriteLine("végeztem " + n);
        }

        public override string ToString() {
            return "RandomAvgTask{Color=" + Color + ",n=" + n + "}";
        }
    }
}