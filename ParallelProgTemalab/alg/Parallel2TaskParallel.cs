using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelProgTemalab.alg
{
    public class Parallel2TaskParallel : MatrixMultAlgorithm
    {
        private Parallel2TaskParallel() { }
        public static Parallel2TaskParallel Algorithm = new Parallel2TaskParallel();
        public string Name => "Parallel2TaskParallel";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            
            var size = m1.Size;
            var mtx = new Matrix(size);

            Parallel.For(0, size, delegate(int row)
            {
                for (int k = 0; k < size; k++)
                    for (int col = 0; col < size; col++)
                        mtx[row, col] += m1[row, k] * m2[k, col];
            });
            
            return mtx;
        }


    }
}