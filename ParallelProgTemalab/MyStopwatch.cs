using System;
using System.Diagnostics;
using System.Dynamic;

namespace ParallelProgTemalab
{
    public class MyStopwatch : IDisposable
    {
        private readonly string name;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly bool printOnStop;
        private double elapsedSeconds = Double.NaN;
        
        public double ElapsedSeconds => elapsedSeconds;

        public MyStopwatch() : this("", false) { }

        public MyStopwatch(string name, bool printOnStop = true)
        {
            (this.name, this.printOnStop) = (name, printOnStop);
            stopwatch.Start();
        }
        
        public void Dispose()
        {
            stopwatch.Stop();
            elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000.0;
            if (printOnStop)
                Console.WriteLine($"Stopwatch [{name}]: {elapsedSeconds}s");
        }
    }
}