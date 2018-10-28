# 3. feladat: tömb rendezés parallel merge sort alrogitmussal C# nyelven

Az összes mérést 1 000 000 elemű listával végeztem.

Az egyszálú és párhuzamos algoritmusok összehasonlítása:

![](results/diagram4.png)


Előzetesen arra számítottam, hogy az optimum 1000 körülre fog kijönni, de a grafikonon a minimum 240000 körül van.
A méréseket természetesen többször végeztem el (20 futtatás, 4 legkisebb és 4 legnagyobb értéket eldobva)

![](results/diagram3_200000-300000.png)

![](results/diagram1_2-1000000.png)

![](results/diagram5_2-1000.png)


A gépem adatai, amin a mérést végeztem:

- i7-6700HQ
- 4 fizikai mag
- 8 logikai mag
- 8GB RAM
