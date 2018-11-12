package sort
import java.util.concurrent.*


fun mergeSort(array: IntArray)
        = mergeSort(array, 0, array.size - 1, IntArray(array.size))

fun parallelMergeSort(array: IntArray, parallelThreshold: Int) {
    parallelMergeSort(array, 0, array.size - 1, IntArray(array.size), parallelThreshold)
}


//val executorService = Executors.newFixedThreadPool(Runtime.getRuntime().availableProcessors() + 1)

val forkJoinPool = ForkJoinPool() // #cores darab szálat indít alapból
// azért nem az optimális #cores+1, mert szükség van 1 szálra, ami a fork-join műveletekért felel

private fun mergeSort(array: IntArray, leftStart: Int, rightEnd: Int, temp: IntArray) {
    if (rightEnd > leftStart) {
        val middle = (leftStart + rightEnd) / 2
        mergeSort(array, leftStart, middle, temp)
        mergeSort(array, middle + 1, rightEnd, temp)
        mergeHalves(array, leftStart, rightEnd, temp)
    }
}

private fun parallelMergeSort(array: IntArray, leftStart: Int, rightEnd: Int, temp: IntArray, parallelThreshold: Int) {
    if (rightEnd > leftStart) {
        if (rightEnd - leftStart >= parallelThreshold) {
            val middle = (leftStart + rightEnd) / 2
            /*val job1 = executorService.submit { parallelMergeSort(array, leftStart, middle, temp, parallelThreshold) }
            val job2 = executorService.submit { parallelMergeSort(array, middle + 1, rightEnd, temp, parallelThreshold) }
            job1.get()
            job2.get()*/
            forkJoinPool.invokeAll(listOf(
                Callable { parallelMergeSort(array, leftStart, middle, temp, parallelThreshold) },
                Callable { parallelMergeSort(array, middle + 1, rightEnd, temp, parallelThreshold) }
            ))
            mergeHalves(array, leftStart, rightEnd, temp)
        } else {
            mergeSort(array, leftStart, rightEnd, temp)
        }
    }
}


// TODO parallelize / in-place
private fun mergeHalves(array: IntArray, leftStart: Int, rightEnd: Int, temp: IntArray) {
    val leftEnd = (rightEnd + leftStart) / 2
    val rightStart = leftEnd + 1
    val size = rightEnd - leftStart + 1

    var left = leftStart
    var right = rightStart
    var i = leftStart

    while (left <= leftEnd && right <= rightEnd) {
        val arrayLeft = array[left]
        val arrayRight = array[right]
        if (arrayLeft <= arrayRight) {
            temp[i] = arrayLeft
            left++
        } else {
            temp[i] = arrayRight
            right++
        }
        i++
    }

    System.arraycopy(array, left, temp, i, leftEnd - left + 1)
    System.arraycopy(array, right, temp, i, rightEnd - right + 1)
    System.arraycopy(temp, leftStart, array, leftStart, size)

}


private class ParallelMergeSort(val array: IntArray,
                                val leftStart: Int,
                                val rightEnd: Int,
                                val temp: IntArray,
                                val parallelThreshold: Int)
    : RecursiveAction() {


    override fun compute() {
        if (rightEnd > leftStart) {
            if (rightEnd - leftStart > parallelThreshold) {
                val middle = (leftStart + rightEnd) / 2
                invokeAll(
                    ParallelMergeSort(array, leftStart, middle, temp, parallelThreshold), // left
                    ParallelMergeSort(array, middle + 1, rightEnd, temp, parallelThreshold) // right
                )
                mergeHalves(array, leftStart, rightEnd, temp)
            } else {
                mergeSort(array, leftStart, rightEnd, temp)
            }
        }
    }

}