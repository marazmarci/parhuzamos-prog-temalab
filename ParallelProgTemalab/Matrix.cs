using System;
using System.Text;

namespace ParallelProgTemalab
{
    public class Matrix
    {
        private int size;
        private int[,] data;

        public int Size => size;

        public Matrix(int size) : this(new int[size,size]) { }

        public Matrix(int[,] data)
        {
            this.size = data.GetLength(0);
            this.data = data;
        }

        public int this[int row, int col]
        {
            get => data[row, col];
            set => data[row, col] = value;
        }

        public static Matrix GenerateRandomMatrix(int size, Random random = null)
        {
            if (random == null)
                random = new Random();
            var mtx = new Matrix(size);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    mtx[i, j] = random.Next(2000) - 1000;
            return mtx;
        }
        
        public Matrix Multiply(Matrix other, MatrixMultiplicationAlgorithm alg)
            => alg.Multiply(this, other);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Matrix [{size}x{size}] {{");
            for (int i = 0; i < size; i++)
            {
                sb.Append("    ");
                for (int j = 0; j < size; j++)
                {
                    sb.Append(data[i, j]);
                    if (j < size - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}