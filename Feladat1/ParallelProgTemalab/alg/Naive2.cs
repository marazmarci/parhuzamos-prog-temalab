using System;
using System.Diagnostics;

namespace ParallelProgTemalab.alg
{
    public class Naive2 : MatrixMultAlgorithm
    {
        private Naive2() { }
        public static Naive2 Algorithm = new Naive2();
        public string Name => "Naive2";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            var size = m1.Size;
            var mtx = new Matrix(size);
            for (int row = 0; row < size; row++)
                for (int i = 0; i < size; i++)
                    for (int col = 0; col < size; col++)
                        mtx[row, col] += m1[row, i] * m2[i, col];
            return mtx;
        }
        
    }
}