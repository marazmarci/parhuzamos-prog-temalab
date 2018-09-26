using System;
using System.Diagnostics;

namespace ParallelProgTemalab.alg
{
    public class Naive2 : MatrixMultiplicationAlgorithm
    {
        private Naive2() { }
        public static Naive2 Algorithm = new Naive2();
        public string Name => "Naive2";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            var size = m1.Size;
            var mtx = new Matrix(size);
            for (int i = 0; i < size; i++)
                for (int k = 0; k < size; k++)
                    for (int j = 0; j < size; j++)
                        mtx[i, j] += m1[i, k] * m2[k, j];
            return mtx;
        }
        
    }
}