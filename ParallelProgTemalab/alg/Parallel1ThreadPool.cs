using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace ParallelProgTemalab.alg
{
    public class Parallel1ThreadPool : MatrixMultAlgorithm
    {
        private Parallel1ThreadPool() { }
        public static Parallel1ThreadPool Algorithm = new Parallel1ThreadPool();
        public string Name => "Parallel1ThreadPool";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            
            var size = m1.Size;
            var mtx = new Matrix(size);

            using (var countdown = new CountdownEvent(size))
            {
                for (int row_ = 0; row_ < size; row_++)
                {
                    int row = row_; // hogy a worker thread ne a folyton megváltozó row-t lássa
                    ThreadPool.QueueUserWorkItem( _ =>
                    {
                        for (int k = 0; k < size; k++)
                            for (int col = 0; col < size; col++)
                                mtx[row, col] += m1[row, k] * m2[k, col];
                        
                        countdown.Signal();
                    });
                }
                
                countdown.Wait();
            }
            
            return mtx;
        }
        

        public static void SetMaxThreads(int numThreads)
        {
            ThreadPool.GetMaxThreads(out int asd, out int completionPortThreads);
            ThreadPool.SetMaxThreads(numThreads, completionPortThreads);
        }



    }
}