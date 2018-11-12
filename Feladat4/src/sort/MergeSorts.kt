package sort
import java.util.concurrent.*
import kotlin.math.max


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
        val length = rightEnd - leftStart
        if (length >= parallelThreshold) {
            val middle = (leftStart + rightEnd) / 2
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


private class ParallelMergeSort(
    val array: IntArray,
    val leftStart: Int,
    val rightEnd: Int,
    val temp: IntArray,
    val parallelThreshold: Int
) : RecursiveAction() {


    override fun compute() {
        if (rightEnd > leftStart) {
            if (rightEnd - leftStart > parallelThreshold) {
                val middle = (leftStart + rightEnd) / 2
                invokeAll(
                    ParallelMergeSort(array, leftStart, middle, temp, parallelThreshold), // left
                    ParallelMergeSort(array, middle + 1, rightEnd, temp, parallelThreshold) // r
                )
                mergeHalves(array, leftStart, rightEnd, temp)
            } else {
                mergeSort(array, leftStart, rightEnd, temp)
            }
        }
    }

}


private fun myBinarySearch(value: Int, array: IntArray, left: Int, right: Int): Int {
    var low = left
    var high = max(left, right + 1)
    while (low < high) {
        val mid = (low + high) / 2
        if (value <= array[mid]) {
            high = mid
        } else {
            low = mid + 1
        }
    }
    return high
}

private fun mergeInPlace(t: IntArray, l: Int, m: Int, r: Int) {
    val length1 = m - l + 1
    val length2 = r - m
    if (length1 >= length2) {
        if (length2 <= 0)
            return
        val q1 = (m + 1) / 2
        val q2 = myBinarySearch(t[q1], t, m + 1, r)
        val q3 = q1 + (q2 - m - 1)
        blockSwapGriesAndMills(t, q1, m, q2 - 1)
        mergeInPlace(t, l, q1 - 1, q3 - 1)
        mergeInPlace(t, q3 + 1, q2 - 1, r)
    } else {
        if (length1 <= 0)
            return
        val q1 = (m + 1 + r) / 2
        val q2 = myBinarySearch(t[q1], t, l, m)
        val q3 = q2 + (q1 - m - 1)
        blockSwapGriesAndMills(t, q2, m, q1)
        mergeInPlace(t, l, q2 - 1, q3 - 1)
        mergeInPlace(t, q3 + 1, q1, r)
    }
}

private class ParallelMergeInPlace(
    val t: IntArray,
    val l: Int,
    val m: Int,
    val r: Int
) : RecursiveAction() {

    override fun compute() {
        val length1 = m - l + 1
        val length2 = r - m
        if (length1 + length2 <= 1024) {
            mergeInPlace(t, l, m + 1, r + 1)
            return
        }
        if (length1 >= length2) {
            if (length2 <= 0)
                return
            val q1 = (m + 1) / 2
            val q2 = myBinarySearch(t[q1], t, m + 1, r)
            val q3 = q1 + (q2 - m - 1)
            blockSwapGriesAndMills(t, q1, m, q2 - 1)
            ForkJoinTask.invokeAll(
                ParallelMergeInPlace(t, l, q1 - 1, q3 - 1),
                ParallelMergeInPlace(t, q3 + 1, q2 - 1, r)
            )
        } else {
            if (length1 <= 0)
                return
            val q1 = (m + 1 + r) / 2
            val q2 = myBinarySearch(t[q1], t, l, m)
            val q3 = q2 + (q1 - m - 1)
            blockSwapGriesAndMills(t, q2, m, q1)
            ForkJoinTask.invokeAll(
                ParallelMergeInPlace(t, l, q2 - 1, q3 - 1),
                ParallelMergeInPlace(t, q3 + 1, q1, r)
            )
        }
    }

}


private fun blockSwapGriesAndMills(a: IntArray, l: Int, m: Int, r: Int) {
    val rotdist = m - l + 1
    val n = r - l + 1
    if (rotdist == 0 || rotdist == n)
        return
    val p = rotdist
    var i = p
    var j = n - p
    while (i != j) {
        if (i > j) {
            swapTwoSequentialSubArrays(a, p - i, p, j)
            i -= j
        } else {
            swapTwoSequentialSubArrays(a, p - i, p + j - i, i)
            j -= i
        }
    }
    swapTwoSequentialSubArrays(a, p - i, p, i)
}


private fun swapTwoSequentialSubArrays(x: IntArray, a: Int, b: Int, m: Int) {
    var mm = m
    var aa = a
    var bb = b
    while (mm-- > 0)
        swapInArray(x, aa++, bb++)
}