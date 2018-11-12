# 4. feladat: tömb rendezés parallel merge sort alrogitmussal választott (Kotlin) nyelven

Az összes mérést 1 000 000 elemű listával végeztem.

Az egyszálú és párhuzamos algoritmusok összehasonlítása:

TODO


```
private fun parallelMergeSort(array: IntArray, leftStart: Int, rightEnd: Int, temp: IntArray, parallelThreshold: Int) {
    if (rightEnd > leftStart) {
```     if (rightEnd - leftStart >= parallelThreshold) {
```
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
```

![](results/futasidok.png)


A gépem adatai, amin a mérést végeztem:

- i7-6700HQ
- 4 fizikai mag
- 8 logikai mag
- 8GB RAM
