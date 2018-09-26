using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using ParallelProgTemalab.alg;

namespace ParallelProgTemalab
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Add meg az összeszorzandó mátrixok méretét! ");
            string input = Console.ReadLine();
            Console.WriteLine();
            if (!Int32.TryParse(input, out int size))
                return;
            
            Matrix m1 = Matrix.GenerateRandomMatrix(size);
            Matrix m2 = Matrix.GenerateRandomMatrix(size);
            Matrix result;
            
            using (new MyStopwatch("naive1"))
            {
                result = m1.Multiply(m2, Naive1.Algorithm);
            }
            
            //Console.WriteLine(result);
            
            using (new MyStopwatch("naive2"))
            {
                result = m1.Multiply(m2, Naive2.Algorithm);
            }
            
            //Console.WriteLine(result);
            
            
        }
    }
}