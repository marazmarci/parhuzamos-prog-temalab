using System;
using System.Diagnostics;

namespace ParallelProgTemalab.alg
{
    public class Naive1 : MatrixMultiplicationAlgorithm
    {
        private Naive1() { }
        public static Naive1 Algorithm = new Naive1();
        public string Name => "Naive1";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            var size = m1.Size;
            var mtx = new Matrix(size);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    for (int k = 0; k < size; k++)
                        mtx[i, j] += m1[i, k] * m2[k, j];
            return mtx;
        }
        
    }
}