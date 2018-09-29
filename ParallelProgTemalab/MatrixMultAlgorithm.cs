namespace ParallelProgTemalab
{
    
    public interface MatrixMultAlgorithm
    {
        
        Matrix Multiply(Matrix m1, Matrix m2);

        string Name { get; }

    }
    
}