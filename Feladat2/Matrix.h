#pragma once
#include "pch.h"
#include <type_traits>
#include <iostream>
#include <cstdlib>

class Matrix {
	int* rowMajorData;
	int** rows;
	//int* refCount;
public:
	const size_t size;
	Matrix(size_t size, int* rowMajorData);
	Matrix(size_t size);
	const int* operator[](const size_t row) const;
	int* operator[](const size_t row);
	void freeMemory();
	friend std::ostream& operator<<(std::ostream& os, const Matrix& m);
	static Matrix genRandomMtx(size_t size);
};