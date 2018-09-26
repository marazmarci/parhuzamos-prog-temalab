namespace ParallelProgTemalab
{
    
    public interface MatrixMultiplicationAlgorithm
    {
        
        Matrix Multiply(Matrix m1, Matrix m2);

        string Name { get; }

    }
    
}