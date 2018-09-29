using System;
using System.Diagnostics;

namespace ParallelProgTemalab.alg
{
    public class Naive1 : MatrixMultAlgorithm
    {
        private Naive1() { }
        public static Naive1 Algorithm = new Naive1();
        public string Name => "Naive1";

        public Matrix Multiply(Matrix m1, Matrix m2)
        {
            Debug.Assert(m1.Size == m2.Size);
            var size = m1.Size;
            var mtx = new Matrix(size);
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    for (int i = 0; i < size; i++)
                        mtx[row, col] += m1[row, i] * m2[i, col];
            return mtx;
        }
        
    }
}