#include "pch.h"
#include "mtx_mult_alg.h"
#include <windows.h>
#include <vector>
#include <map>
#include <algorithm>
#include <numeric>
#include <string>


const unsigned int MAX_MTX_SIZE = 200;

const unsigned int REPETITIONS = 28;
const unsigned int FILTER_MIN_MAX = 6;





int main() {

	std::vector<MatrixMultiplicationAlgorithm*> algorithms{
		new Naive1Algorithm,
		new Naive2Algorithm,
		new ThreadPoolAlgorithm,
		new ParallelForAlgorithm
	};

	// warm up ThreadPools
	for (size_t i = 2; i <= 3; i++)
		algorithms[i]->Multiply(Matrix::genRandomMtx(333), Matrix::genRandomMtx(333));

	std::cout << "size";
	std::for_each(algorithms.begin(), algorithms.end(), [&](MatrixMultiplicationAlgorithm* alg) {
		std::cout << ";" << alg->name;
	});
	std::cout << std::endl;


	for (size_t size = 1; size <= MAX_MTX_SIZE; size++) {
		
		std::cout << size;

		std::for_each(algorithms.begin(), algorithms.end(), [&](MatrixMultiplicationAlgorithm* alg) {
			//results[alg] = std::map<size_t, unsigned int>();

			std::vector<unsigned int> times;
			for (int i = 0; i < REPETITIONS; i++) {
				auto mtx1 = Matrix::genRandomMtx(size);
				auto mtx2 = Matrix::genRandomMtx(size);

				LARGE_INTEGER begin = { 0 }, end = { 0 };
				QueryPerformanceCounter(&begin);
				Matrix result = alg->Multiply(mtx1, mtx2);
				QueryPerformanceCounter(&end);
				auto elapsed = (unsigned long)(end.QuadPart - begin.QuadPart);

				mtx1.freeMemory();
				mtx2.freeMemory();
				result.freeMemory();

				times.emplace_back(elapsed);
			}
			std::sort(times.begin(), times.end());
			times.erase(times.begin(), times.begin() + FILTER_MIN_MAX);
			times.erase(times.end() - FILTER_MIN_MAX, times.end());
			auto time = (unsigned int)(std::accumulate(times.begin(), times.end(), 0L) / (unsigned long)times.size());
			//results[alg][size] = time;
			std::cout << ';' << time;
			//std::cout << alg->name << ", size = " << size << ", time = " << time << std::endl;

		});

		std::cout << std::endl;

	}

}



/*
	Matrix testMtx1(3, new int[9]{
		1, 2, 3,
		4, 5, 6,
		7, 8, 9
		});

	Matrix testMtx2(3, new int[9]{
		2, 2, 2,
		2, 2, 2,
		2, 2, 2
		});

	Matrix eig(3, new int[9]{
		1, 0, 0,
		0, 1, 0,
		0, 0, 1
		});

	std::cout << " - Naive1:" << std::endl;
		auto result1 = Naive1().Multiply(mtx1, mtx2);
	std::cout << " - Naive2:" << std::endl;
		auto result2 = Naive2().Multiply(mtx1, mtx2);
	std::cout << " - Threads:" << std::endl;
		auto result3 = Threads().Multiply(mtx1, mtx2);*/