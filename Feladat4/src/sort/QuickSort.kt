package sort


fun quickSort(array: IntArray)
    = quickSort(array, 0, array.size - 1)

private fun quickSort(array: IntArray, start: Int, end: Int) {
    if (end > start) {
        val pivot = findGoodPivot(array, start, end)
        val newPivot = partition(array, start, end, pivot)
        quickSort(array, start, newPivot - 1)
        quickSort(array, newPivot + 1, end)
    }
}

private fun findGoodPivot(array: IntArray, start: Int, end: Int): Int {

    var aIdx = start
    var a = array[aIdx]
    var bIdx = (start + end) / 2
    var b = array[bIdx]
    var cIdx = end
    var c = array[cIdx]

    fun swapAB() {
        if (a > b) {
            a = b.also { b = a }
            aIdx = bIdx.also { bIdx = aIdx }
        }
    }
    swapAB()
    if (b > c) {
        b = c.also { c = b }
        bIdx = cIdx.also { cIdx = bIdx }
    }
    swapAB()

    return bIdx
}

private fun partition(array: IntArray, start: Int, end: Int, pivot: Int): Int {
    val pivotVal = array[pivot]
    swapInArray(array, pivot, end)
    var storeIdx = start
    for (i in start until end) {
        if (array[i] < pivotVal) { // array[i].CompareTo(pivotVal) < 0
            swapInArray(array, i, storeIdx)
            storeIdx++
        }
    }
    swapInArray(array, storeIdx, end)
    return storeIdx
}

private fun swapInArray(array: IntArray, i1: Int, i2: Int) {
    val temp = array[i1]
    array[i1] = array[i2]
    array[i2] = temp
}