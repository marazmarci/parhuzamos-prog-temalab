import java.io.FileWriter
import java.util.*


lateinit var rand: Random

fun main() {

    resetRandom()

    val scanner = Scanner(System.`in`)

    print("Add meg a rendezendő lista méretét!  ")
    val size = scanner.nextIntLineOrDefault(1_000_000)
    println()

    print("Melyik algoritmus? (0: mindegyik, 1: QuickSort, 2: MergeSort, 3: ParallelMergeSort)  ")
    val alg = scanner.nextIntLineOrDefault(0)
    println()

    val quickSort = alg in arrayOf(0,1)
    val mergeSort = alg in arrayOf(0,2)
    val parallelMergeSort = alg in arrayOf(0,3)

    val arrayInitializerLambda = { generateRandomSequentialArray(size) }

    if (quickSort) {
        resetRandom()
        val time = measureNanoSeconds(arrayInitializerLambda) {
            sort.quickSort(it)
        }
        println("QuickSort finished: ${time.nsToMs()} ms")
        FileWriter("results\\single_run\\QuickSort.txt").use { it.write("${time.nsToMs()}") }
    }

    if (mergeSort) {
        resetRandom()
        val time = measureNanoSeconds(arrayInitializerLambda) {
            sort.mergeSort(it)
        }
        println("MergeSort finished: ${time.nsToMs()} ms")
        FileWriter("results\\single_run\\MergeSort.txt").use { it.write("${time.nsToMs()}") }
    }

    if (parallelMergeSort) {
        resetRandom()
        val time = measureNanoSeconds(arrayInitializerLambda) {
            sort.parallelMergeSort(it, 8192)
        }
        println("ParallelMergeSort (single run) finished: ${time.nsToMs()} ms")
        FileWriter("results\\single_run\\ParallelMergeSort.txt").use { it.write("${time.nsToMs()}") }
    }

    if (parallelMergeSort) {
        val fixArray = arrayInitializerLambda()
        println("ParallelMergeSort starting...")
        val results = mutableMapOf<Int,Long>()

        val parallelThresholds = TreeSet<Int>().apply {
            var n1 = size - 1 // 999_999
            var n2 = n1
            do {
                n1 /= 2
                n2 = (n2-1) / 2
                for (eps in -3..3) {
                    add(n1 + eps)
                    add(n2 + eps)
                }
            } while (n1 > 500 && n2 > 500)
        }


        val initLambda = { fixArray.clone() }

        FileWriter("results\\ParallelMergeSort.csv").use { fw ->
            parallelThresholds.forEach { parallelThreshold ->
                val time = measureNanoSeconds(initLambda) {
                    sort.parallelMergeSort(it, parallelThreshold)
                }
                results[parallelThreshold] = time
                println("parallelThreshold = $parallelThreshold finished: ${time.nsToMs()} ms")
                fw.write("$parallelThreshold;${time.nsToMs()}\n")
                fw.flush()
            }
        }

    }


}



fun <T>measureNanoSeconds(init: () -> T, repeats: Int = 16, filterRepeats: Int = 4, toMeasure: (T) -> Unit): Long {
    val times = ArrayList<Long>(repeats)
    repeat(repeats) {
        val array = init.invoke()
        val time = stopwatch {
            toMeasure.invoke(array)
        }
        times.add(time)
    }
    times.filterTimeMeasurements(filterRepeats)
    return times.sum() / times.size
}

fun generateRandomSequentialArray(size: Int) = IntArray(size) { i-> i }.apply { shuffle() }

fun IntArray.shuffle() {
    var n = size
    while (n > 1) {
        val k = rand.nextInt(n)
        n--
        val t = this[n]
        this[n] = this[k]
        this[k] = t
    }
}

fun MutableList<Long>.filterTimeMeasurements(repeats: Int) = repeat(repeats) {
    remove(max())
    remove(min())
}


fun IntArray.print() {
    forEach { print("$it ") }
    println()
}

fun resetRandom() {
    rand = Random(42)
}

fun Long.nsToMs() = this * 1e-6

fun Scanner.nextIntLineOrDefault(default: Int) = nextLine().toIntOrNull() ?: default