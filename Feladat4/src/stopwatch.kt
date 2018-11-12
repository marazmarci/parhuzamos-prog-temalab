



inline fun stopwatch(crossinline lambda: () -> Unit): Long {
    val start = System.nanoTime()
    lambda()
    val end = System.nanoTime()
    return end - start
}