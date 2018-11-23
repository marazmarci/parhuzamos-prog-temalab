# 4. feladat: tömb rendezés parallel merge sort alrogitmussal választott (Kotlin) nyelven

Az algoritmust a javasolt ExecutorService helyett a ForkJoinPool és a RecursiveAction beépített Java osztályokkal implementáltam. A ForkJoinPool fork-join feladatokra van optimalizálva, mint ahogy az a nevéből is látszik. A RecursiveAction pedig a ForkJoinTask osztályból származik le, és rekurzív párhuzamos algoritmusokra van optimalizálva. Először ExecutorService-szel írtam meg, és akkor kb. 2x annyi ideig futott az algoritmus.

Az összes mérést 1 000 000 elemű listával végeztem.

Az egyszálú és párhuzamos algoritmusok összehasonlítása:

![](results/singlethread_vs_parallel.png)

![](results/parallelThreshold_code.png)

Rájöttem, hogy a képen látható length változó csak kevés fajta értéket vehet fel, és ezek az értékek a rendezendő tömb hosszától függenek.

(1000000 méretű tömb esetén: 1, 2, 3, 6, 7, 14, 15, 29, 30, 60, 61, 121, 122, 243, 244, 487, 488, 975, 976, 1952, 1953, 3905, 3906, 7811, 7812, 15624, 31249, 62499, 124999, 249999, 499999)

(Ez amiatt van, mert az algoritmus mindig felezi az intervallumokat.)

Ebből kifolyólag csak az ezen számok kicsi környezetében lévő parallelThreshold értékekre futtattam a benchmarkot, mert pl. egy tetszőleges 1000-es értékre ugyanazt az eredményt kapnám, mint a 976-ra, vagy az 1951-re.

(ezeket a számokat természetesen nem hardcode-oltam bele a programba, hanem kiszámoltattam vele, hogy ne csak 1000000-s tömbméretre működjön.)

![](results/paralellmergesort_benchmark.png)

(az X tengely logaritmikusan skálázott)

parallelThreshold: az a tömbméret, ami alatt már egy szálú merge sort fut le.


### Az optimum a diagram alapján: parallelThreshold = 17


A gépem adatai, amin a mérést végeztem:

- i7-6700HQ
- 4 fizikai mag
- 8 logikai mag
- 8GB RAM
