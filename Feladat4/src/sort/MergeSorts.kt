package sort
import java.util.concurrent.*
import kotlin.math.max


fun mergeSort(array: IntArray)
        = mergeSort(array, 0, array.size - 1, IntArray(array.size))

fun parallelMergeSort(array: IntArray, parallelThreshold: Int) {
    //parallelMergeSort(array, 0, array.size - 1, IntArray(array.size), parallelThreshold)
    //parallelMergeSortInPlace(array, 0, array.size - 1, parallelThreshold)
    //forkJoinPool.invoke(ParallelMergeSortInPlace(array, 0, array.size - 1, parallelThreshold))
    forkJoinPool.invoke(ParallelMergeSort(array, 0, array.size - 1, IntArray(array.size), parallelThreshold))
}


val forkJoinPool = ForkJoinPool() // #cores darab szálat indít alapból
// azért nem a ThreadPool-oknál megszokott optimális #cores+1, mert szükség van 1 szálra, ami a fork-join műveletekért felel


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

private fun parallelMergeSortInPlace(array: IntArray, leftStart: Int, rightEnd: Int, parallelThreshold: Int) {
    if (rightEnd > leftStart) {
        val length = rightEnd - leftStart
        if (length >= parallelThreshold) {
            val middle = (leftStart + rightEnd) / 2
            forkJoinPool.invokeAll(listOf(
                Callable { parallelMergeSortInPlace(array, leftStart, middle, parallelThreshold) },
                Callable { parallelMergeSortInPlace(array, middle + 1, rightEnd, parallelThreshold) }
            ))
            mergeInPlace(array, leftStart, middle, rightEnd)
        } else {
            mergeSortInPlace(array, leftStart, rightEnd)
        }
    }
}



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

private fun mergeSortInPlace(a: IntArray, l: Int, r: Int) {
    if (r <= l)
        return
    val m = (l + r) / 2
    mergeSortInPlace(a, l, m)
    mergeSortInPlace(a, m + 1, r)
    mergeInPlace(a, l, m, r)
}


private class ParallelMergeSort(
    val a: IntArray,
    val l: Int,
    val r: Int,
    val temp: IntArray,
    val parallelThreshold: Int
) : RecursiveAction() {


    override fun compute() {
        if (r <= l)
            return
        if (r - l > parallelThreshold) {
            val m = (l + r) / 2
            invokeAll(
                ParallelMergeSort(a, l, m, temp, parallelThreshold), // left
                ParallelMergeSort(a, m + 1, r, temp, parallelThreshold) // right
            )
            //forkJoinPool.invoke(ParallelMergeInPlace(a, l, middle, r))
            //quietlyInvoke()
            //invoke()
            //invokeAll(ParallelMergeInPlace(a, l, m, r))
            mergeHalves(a, l, r, temp)
        } else {
            mergeSort(a, l, r, temp)
        }
    }

}


/* Forrás:
     http://www.drdobbs.com/parallel/parallel-in-place-merge-sort/240169094?pgno=1
     http://www.drdobbs.com/parallel/parallel-in-place-merge-sort/240169094?pgno=2
 */

private class ParallelMergeSortInPlace(
    val a: IntArray,
    val l: Int,
    val r: Int,
    //val temp: IntArray,
    val parallelThreshold: Int
) : RecursiveAction() {


    override fun compute() {
        if (r <= l)
            return
        if (r - l > parallelThreshold) {
            val m = (l + r) / 2
            invokeAll(
                ParallelMergeSortInPlace(a, l, m, parallelThreshold), // left
                ParallelMergeSortInPlace(a, m + 1, r, parallelThreshold) // right
            )
            //forkJoinPool.invoke(ParallelMergeInPlace(a, l, middle, r))
            //quietlyInvoke()
            //invoke()
            invokeAll(ParallelMergeInPlace(a, l, m, r))
            //mergeHalves(a, l, r, temp)
        } else {
            //mergeSort(a, l, r, temp)
            mergeSortInPlace(a, l, r)
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
        val q1 = (l + m) / 2
        val q2 = myBinarySearch(t[q1], t, m + 1, r)
        val q3 = q1 + (q2 - m - 1)
        blockExchangeJugglingBentley(t, q1, m, q2 - 1)
        mergeInPlace(t, l, q1 - 1, q3 - 1)
        mergeInPlace(t, q3 + 1, q2 - 1, r)
    } else {
        if (length1 <= 0)
            return
        val q1 = (m + 1 + r) / 2
        val q2 = myBinarySearch(t[q1], t, l, m)
        val q3 = q2 + (q1 - m - 1)
        blockExchangeJugglingBentley(t, q2, m, q1)
        mergeInPlace(t, l, q2 - 1, q3 - 1)
        mergeInPlace(t, q3 + 1, q1, r)
    }
}

/* Forrás:
     http://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=1
     http://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=2
     http://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=3
     http://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=4
 */

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
            val q1 = (l + m) / 2
            val q2 = myBinarySearch(t[q1], t, m + 1, r)
            val q3 = q1 + (q2 - m - 1)
            blockExchangeJugglingBentley(t, q1, m, q2 - 1)
            invokeAll(
                ParallelMergeInPlace(t, l, q1 - 1, q3 - 1),
                ParallelMergeInPlace(t, q3 + 1, q2 - 1, r)
            )
        } else {
            if (length1 <= 0)
                return
            val q1 = (m + 1 + r) / 2
            val q2 = myBinarySearch(t[q1], t, l, m)
            val q3 = q2 + (q1 - m - 1)
            blockExchangeJugglingBentley(t, q2, m, q1)
            invokeAll(
                ParallelMergeInPlace(t, l, q2 - 1, q3 - 1),
                ParallelMergeInPlace(t, q3 + 1, q1, r)
            )
        }
    }

}


/* Forrás:
     http://www.drdobbs.com/parallel/benchmarking-block-swapping-algorithms/232900395?pgno=1
     http://www.drdobbs.com/parallel/benchmarking-block-swapping-algorithms/232900395?pgno=2
     http://www.drdobbs.com/parallel/benchmarking-block-swapping-algorithms/232900395?pgno=3
 */

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

private fun blockExchangeJugglingBentley(a: IntArray, l: Int, m: Int, r: Int) {
    val uLength = m - l + 1
    val vLength = r - m
    if (uLength <= 0 || vLength <= 0) return
    val rotdist = m - l + 1
    val n = r - l + 1
    val gcdRotdistN = gcd(rotdist, n)
    for (i in 0 until gcdRotdistN) {
        // move i-th values of blocks
        val t = a[i]
        var j = i
        while (true) {
            var k = j + rotdist
            if (k >= n)
                k -= n
            if (k == i)
                break
            a[j] = a[k]
            j = k
        }
        a[j] = t
    }
}


private fun swapTwoSequentialSubArrays(x: IntArray, a: Int, b: Int, m: Int) {
    var mm = m
    var aa = a
    var bb = b
    while (mm-- > 0)
        swapInArray(x, aa++, bb++)
}

private fun gcd(ii: Int, jj: Int): Int {
    var i = ii
    var j = jj
    if (i == 0) return i
    if (j == 0) return j
    while (i != j) {
        if (i > j)
            i -= j
        else
            j -= i
    }
    return i
}