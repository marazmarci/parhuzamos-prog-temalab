#include "pch.h"
#include "mtx_mult_alg.h"
#include <windows.h>
#include <ppl.h>
#include <concurrent_vector.h>
#include <array>
#include <vector>
#include <tuple>
#include <algorithm>
#include <iostream>
#include <thread> 
#include <tuple>

using namespace concurrency;


Matrix Naive1Algorithm::multiply(const size_t size, Matrix m1, Matrix m2) {
	Matrix result{ size, new int[size*size] };
	for (size_t row = 0; row < size; row++)
		for (size_t col = 0; col < size; col++)
			for (size_t k = 0; k < size; k++)
				result[row][col] += m1[row][k] * m2[k][col];
	return result;
}

Matrix Naive2Algorithm::multiply(const size_t size, Matrix m1, Matrix m2) {
	Matrix result(size);
	for (size_t row = 0; row < size; row++)
		for (size_t k = 0; k < size; k++)
			for (size_t col = 0; col < size; col++)
				result[row][col] += m1[row][k] * m2[k][col];
	return result;
}


Matrix ThreadPoolAlgorithm::multiply(const size_t size, Matrix m1, Matrix m2) {

	startThreadPool();
	Matrix result{ size };
	std::vector<std::thread> threads;
	event finishedEvent;
	unsigned int completedRowsCounter = 0;
	std::mutex mutex;

	for (size_t row = 0; row < size; row++) {
		//std::thread thread{ computeRow, row, std::make_tuple(m1, m2), result };
		//threads.push_back(std::move(thread));
		threadPool->enqueue([&, row]() {

			for (size_t k = 0; k < size; k++)
				for (size_t col = 0; col < size; col++)
					result[row][col] += m1[row][k] * m2[k][col];

			std::lock_guard<std::mutex> lock(mutex);
			completedRowsCounter++;
			if (completedRowsCounter == size)
				finishedEvent.set();
			else if (completedRowsCounter > size)
				throw "ez mar tul nagy!!!!";

		});
	}

	finishedEvent.wait();

	return result;
}


void computeRow(const size_t row, std::tuple<Matrix, Matrix> operands, Matrix result) {
	const size_t size = result.size;
	Matrix m1 = std::get<0>(operands);
	Matrix m2 = std::get<1>(operands);
	for (size_t k = 0; k < size; k++)
		for (size_t col = 0; col < size; col++)
			result[row][col] += m1[row][k] * m2[k][col];
}

void ThreadPoolAlgorithm::startThreadPool() {
	if (threadPool == nullptr)
		threadPool = new ThreadPool(GetProcessorCount() + 1);
}


Matrix ParallelForAlgorithm::multiply(const size_t size, Matrix m1, Matrix m2) {

	Matrix result{ size };

	parallel_for(size_t(0), size, [&](size_t row) {
		for (size_t k = 0; k < size; k++)
			for (size_t col = 0; col < size; col++)
				result[row][col] += m1[row][k] * m2[k][col];
	});

	return result;
}