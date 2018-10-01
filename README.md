# 1. feladat: mátrix szorzás C# nyelven

![](ParallelProgTemalab/results/diagram1.png)


![](ParallelProgTemalab/results/diagram2.png)



![](ParallelProgTemalab/results/diagram3.png)

Megismételtem a mérést a milliszekudumos felbontást "tick" felbontásra növelve és így sikerült olyan eredményt kapnom, amiben egy bizonyos mátrixméretig a naiv algoritmusok kicsivel gyorsabbak, mint a párhuzamosak.
A határ láthatóan 29x29-es mátrixméretnél van, így onnantól érdemes párhuzamosítani.



A gépem adatai, amin a mérést végeztem:

- i7-6700HQ
- 4 fizikai mag
- 8 logikai mag
- 8GB RAM
