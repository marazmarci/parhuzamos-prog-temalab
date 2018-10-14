#pragma once
#include "Matrix.h"
#include <string>
#include <concrtrm.h> // GetProcessorCount()
#include "ThreadPool.h" // https://github.com/progschj/ThreadPool


struct MatrixMultiplicationAlgorithm {

	const std::string name;

	MatrixMultiplicationAlgorithm(std::string name) : name{ name } { }

	virtual Matrix Multiply(Matrix m1, Matrix m2) {
		if (m1.size != m2.size)
			throw "Matrices are not the same size!";
		else
			return multiply(m1.size, m1, m2);
	}

  protected:

	virtual Matrix multiply(const size_t size, Matrix m1, Matrix m2) = 0;

};

struct Naive1Algorithm : public MatrixMultiplicationAlgorithm {
	Naive1Algorithm() : MatrixMultiplicationAlgorithm("Naive1") {};
	Matrix multiply(const size_t size, Matrix m1, Matrix m2) override;
};

struct Naive2Algorithm : public MatrixMultiplicationAlgorithm {
	Naive2Algorithm() : MatrixMultiplicationAlgorithm("Naive2") {};
	Matrix multiply(const size_t size, Matrix m1, Matrix m2) override;
};

struct ThreadPoolAlgorithm : public MatrixMultiplicationAlgorithm {
	ThreadPoolAlgorithm() : MatrixMultiplicationAlgorithm("ThreadPool") {};
	Matrix multiply(const size_t size, Matrix m1, Matrix m2) override;
protected:
	ThreadPool* threadPool = nullptr;
	void startThreadPool();
};

struct ParallelForAlgorithm : public MatrixMultiplicationAlgorithm {
	ParallelForAlgorithm() : MatrixMultiplicationAlgorithm("parallel_for") {};
	Matrix multiply(const size_t size, Matrix m1, Matrix m2) override;
};