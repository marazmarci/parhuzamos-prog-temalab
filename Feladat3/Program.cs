using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Feladat3.alg;
using Feladat3.alg.mergesort;
using ParallelProgTemalab;
using System.Diagnostics;

namespace Feladat3 {
    
    class Program {
        
        static Random rand;
        
        static void Main(string[] args) {
            
            using (var p = Process.GetCurrentProcess()) 
                p.PriorityClass = ProcessPriorityClass.RealTime;
            
            resetRandom();
            
            Console.Write("Add meg a rendezendő lista méretét! ");
            string input = Console.ReadLine();
            Console.WriteLine();
            if (!Int32.TryParse(input, out int size))
                size = 1_000_000;
            
            Console.Write("Melyik algoritmus? (0: mindegyik, 1: QuickSort, 2: MergeSort, 3: ParallelMergeSort) ");
            input = Console.ReadLine();
            Console.WriteLine();
            if (!Int32.TryParse(input, out int alg))
                alg = 0;

            bool quickSort = false, mergeSort = false, parallelMergeSort = false;
            switch (alg) {
                case 1: quickSort = true; break;
                case 2: mergeSort = true; break;
                case 3: parallelMergeSort = true; break;
                default: quickSort = mergeSort = parallelMergeSort = true; break;
            }


            var array = generateRandomSequentialArray(size);
            
            
            //printArray(array);

            if (quickSort) {
                resetRandom();
                double time = measureTime(
                    () => array = generateRandomSequentialArray(size),
                    () => QuickSort.Sort(array)
                );
                Console.WriteLine($"QuickSort finished: {time} ticks");
                using (var w = new StreamWriter($"results/QuickSort.txt")) {
                    w.WriteLine(time);
                }
            }

            if (mergeSort) {
                resetRandom();
                double time = measureTime(
                    () => array = generateRandomSequentialArray(size),
                    () => MergeSort.Sort(array)
                );
                Console.WriteLine($"MergeSort finished: {time} ticks");
                using (var w = new StreamWriter($"results/MergeSort.txt")) {
                    w.WriteLine(time);
                }
            }

            if (parallelMergeSort) {
                resetRandom();
                double time = measureTime(
                    () => array = generateRandomSequentialArray(size),
                    () => ParallelMergeSort.Sort(array, 900)
                );
                Console.WriteLine($"ParallelMergeSort finished: {time} ticks");
                using (var w = new StreamWriter($"results/ParallelMergeSort.txt")) {
                    w.WriteLine(time);
                }
            }

            if (parallelMergeSort) {
                
                resetRandom();
                var fixArray = generateRandomSequentialArray(size);
                Console.WriteLine("ParallelMergeSort starting...");
                Dictionary<int, double> results = new Dictionary<int, double>();
                
                var parallelThresholds = new SortedSet<int>();
                var ranges = new List<IEnumerable<int>> {
                    CustomRange(2, 100, 1),
                    CustomRange(100, 1_000, 10),
                    CustomRange(1_000, 3000, 50),
                    //CustomRange(1_000, 2_000, 100),
                    CustomRange(3_000, 10_000, 1000),
                    CustomRange(10_000, 50_000, 10000),
                    CustomRange(50_000, 1_000_000, 50000),
                    CustomRange(240_000, 260_000, 1000),
                    CustomRange(249_900, 250_100, 50),
                    CustomRange(249_990, 250_010, 1),
                    CustomRange(490_000, 510_000, 1000),
                    //CustomRange(5_200_000, 6_000_000, 10000),
                };
                foreach (var range in ranges)
                    foreach (var i in range)
                        parallelThresholds.Add(i);

                Console.WriteLine(parallelThresholds.Count);
                
                using (var fileWriter = new StreamWriter($"results/ParallelMergeSort.csv")) {
                    foreach (int i in parallelThresholds) { // for (int i = 1000; i <= size && i < 10000000; i+=1000) {
                        int parallelThreshold = i;
                        double time = measureTime(
                            () => Array.Copy(fixArray, array, fixArray.Length),
                            () => new MergeSortHelper<int>(parallelThreshold).MergeSort(array, 0, array.Length - 1, true), //ParallelMergeSort.Sort(array, parallelThreshold),
                            20, 4
                        );
                        results.Add(parallelThreshold, time);
                        Console.WriteLine($"parallelThreshold = {parallelThreshold} finished: {time} ticks");
                        fileWriter.WriteLine($"{i};{time}");
                        fileWriter.Flush();
                    }
                }

                //Console.WriteLine($"ParallelMergeSort finished: {time} ticks");

                /*using (var fileWriter = new StreamWriter($"results/ParallelMergeSort.csv")) {
                    for (int i = 0; i < size; i++)
                        fileWriter.WriteLine($"{i};{times[i]}");
                }*/

            }
            

            Console.WriteLine("\nNyomj enter-t a kilépéshez!");
            Console.ReadLine();
            

        }


        static long measureTime(Action init, Action toMeasure, int repeats = 10, int filterRepeats = 2) {
            List<long> times = new List<long>();
            for (int i = 0; i < repeats; i++) {
                init.Invoke();
                MyStopwatch sw;
                using (sw = new MyStopwatch()) {
                    toMeasure.Invoke();
                }
                times.Add(sw.ElapsedTicks);
                //Thread.Sleep(1);
            }
            filterTimeMeasurements(times, filterRepeats);
            return (long) times.Average();
        }
        
        
        private static void filterTimeMeasurements(List<long> times, int repeats) {
            for (int i = 0; i < repeats; i++) {
                times.Remove(times.Max());
                times.Remove(times.Min());
            }
        }
        
        
        static int[] generateRandomIntArray(int size, int max = 99)
        {
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(max);
            return array;
        }

        static int[] generateSequentialArray(int size) {
            var array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = size-i;
            return array;
        }

        static int[] generateRandomSequentialArray(int size) {
            var array = generateSequentialArray(size);
            rand.Shuffle(array);
            return array;
        }

        static void printArray(int[] array) {
            for (int i = 0; i < array.Length; i++)
                Console.Write(array[i] + " ");
            Console.WriteLine();
        }


        static void resetRandom() {
            rand = new Random(42);
        }
        
        // Forrás: https://stackoverflow.com/a/4142644/6347943
        public static IEnumerable<int> CustomRange(int start, int endInclusive, int step) {
            for (int i = start; i <= endInclusive; i += step)
                yield return i;
        }


    }
    
    
    // Forrás: https://stackoverflow.com/a/110570/6347943
    static class RandomExtensions {
        public static void Shuffle<T> (this Random rng, T[] array) {
            int n = array.Length;
            while (n > 1) {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
    
}