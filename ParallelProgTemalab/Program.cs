using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Timers;
using ParallelProgTemalab.alg;

namespace ParallelProgTemalab
{
    
    using BenchmarkResults = List<(MatrixMultAlgorithm, int, double)>;
    
    class Program
    {
        
        private static List<MatrixMultAlgorithm> algorithms = new List<MatrixMultAlgorithm>
        {
            Parallel1ThreadPool.Algorithm,
            Parallel2TaskParallel.Algorithm,
            Naive2.Algorithm,
            Naive1.Algorithm,
        };
        
        
        static void Main(string[] args)
        {
            Console.Write("Add meg az összeszorzandó mátrixok méretét! ");
            string input = Console.ReadLine();
            Console.WriteLine();
            if (!Int32.TryParse(input, out int size))
                return;
            
            
            runBenchmarksAndWriteToFile(size, "results");
            //var results = runBenchmarks(size);
            //printBenchmarkResultsToFile(results, "results");

            
            /*Matrix m1 = Matrix.GenerateRandomMatrix(size);
            Matrix m2 = Matrix.GenerateRandomMatrix(size);
            Matrix result;
            
            using (new MyStopwatch("naive1"))
            {
                result = m1.Multiply(m2, Naive1.Algorithm);
            }
            
            Console.WriteLine(result);
            
            using (new MyStopwatch("naive2"))
            {
                result = m1.Multiply(m2, Naive2.Algorithm);
            }
            
            Console.WriteLine(result);
            
            using (new MyStopwatch("threadpool"))
            {
                result = m1.Multiply(m2, Parallel1ThreadPool.Algorithm);
            }
            
            Console.WriteLine(result);*/
            

        }


        private static void runBenchmarksAndWriteToFile(int maxSize, string folder)
        {
            for (int i = 0; i < algorithms.Count; i++)
            {
                var alg = algorithms[i];
                using (var streamWriter = new StreamWriter($"{folder}/{alg.Name}.csv"))
                {
                    for (int size = 1; size <= maxSize; size++)
                    {
                        double time = measureTime(size, alg);
                        streamWriter.WriteLine($"{size};{time}");
                        Console.WriteLine($"[{i+1}/{algorithms.Count}] {alg.Name}:  {size}  {time}");
                    }
                }
            }
        }



        private static BenchmarkResults runBenchmarks(int maxSize)
        {
            var results = new BenchmarkResults();
            for (int size = 0; size < maxSize; size++)
            {
                foreach (var alg in algorithms)
                    results.Add((alg, size, measureTime(size, alg)));
                Console.WriteLine(size);
            }

            return results;
        }

        private static double measureTime(int size, MatrixMultAlgorithm alg)
        {
            Matrix m1 = Matrix.GenerateRandomMatrix(size);
            Matrix m2 = Matrix.GenerateRandomMatrix(size);
            var times = new List<double>();
            for (int i = 0; i < 9; i++)
            {
                MyStopwatch sw;
                using (sw = new MyStopwatch())
                {
                    m1.Multiply(m2, alg);
                }
                times.Add(sw.ElapsedSeconds);
            }
            filterTimeMeasurements(times);
            double avgTime = times.Sum() / times.Count;
            return avgTime;
        }

        private static void filterTimeMeasurements(List<double> times)
        {
            for (int i = 0; i < 2; i++)
            {
                times.Remove(times.Max());
                times.Remove(times.Min());
            }
        }


        private static void printBenchmarkResultsToFile(BenchmarkResults results, string folder)
        {
            var algResultFileWriters = new Dictionary<MatrixMultAlgorithm, StreamWriter>();

            foreach (var alg in algorithms)
                algResultFileWriters[alg] = new StreamWriter($"{folder}/{alg.Name}.csv");
            
            
            foreach (var result in results)
            {
                var (alg, size, time) = result;
                var stream = algResultFileWriters[alg];
                stream.WriteLine($"{size};{time}");
            }

            
            foreach (var stream in algResultFileWriters.Values)
                stream.Dispose();
        }
        
    }
}