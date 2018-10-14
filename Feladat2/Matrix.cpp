#include "pch.h"
#include "Matrix.h"


Matrix::Matrix(size_t size, int* rowMajorData) : size{ size }, rowMajorData{ rowMajorData }, rows{ new int*[size] } {
	//refCount = new int(1);
	for (size_t i = 0; i < size; i++)
		rows[i] = &rowMajorData[i * size];
}

Matrix::Matrix(size_t size) : Matrix{ size, new int[size*size] } {
	for (size_t i = 0; i < size*size; i++)
		rowMajorData[i] = 0;
}

const int* Matrix::operator[](size_t row) const {
	return rows[row];
}

int* Matrix::operator[](size_t row) {
	return rows[row];
}

std::ostream& operator<<(std::ostream& os, const Matrix& m) {
	os << "Matrix [" << m.size << "x" << m.size << "] {" << std::endl;
	for (size_t i = 0; i < m.size; i++) {
		os << "  ";
		for (size_t j = 0; j < m.size; j++)
			os << ' ' << m.rowMajorData[i * m.size + j];
		os << std::endl;
	}
	return os << '}' << std::endl;
}

/*Matrix::~Matrix() {

	// ezt most elengedem...
	// úgysem a memóóriakezelésen van a hangsúly :)
	// (kéne írni copy meg move ctor-t)

	(*refCount)--;
	if (*refCount == 0) {
		delete rowMajorData;
		delete data;
		delete refCount;
	}
}*/


void Matrix::freeMemory() {
	delete rows;
	delete rowMajorData;
}

Matrix Matrix::genRandomMtx(size_t size) {
	int* data = new int[size*size];
	for (int i = 0; i < size*size; i++)
		data[i] = (std::rand() % 2000) - 1000;
	return Matrix(size, data);
}