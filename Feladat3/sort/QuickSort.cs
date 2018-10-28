using System;

namespace Feladat3.alg {
    
    public class QuickSort {

        public static void Sort(int[] array)
            => quicksort(array, 0, array.Length - 1);
        

        private static void quicksort(int[] array, int start, int end) {
            if (end > start) {
                int pivot = findGoodPivot(array, start, end);
                int newPivot = partition(array, start, end, pivot);
                quicksort(array, start, newPivot - 1);
                quicksort(array, newPivot + 1, end);
            }
        }
        
        private static int findGoodPivot(int[] array, int start, int end) { 
            int aIdx = start;
            int a = array[aIdx];
            int bIdx = (start + end) / 2;
            int b = array[bIdx];
            int cIdx = end;
            int c = array[cIdx];
            
            if (a > b) { swap(ref a, ref b); swap(ref aIdx, ref bIdx); }
            if (b > c) { swap(ref b, ref c); swap(ref bIdx, ref cIdx); }
            if (a > b) { swap(ref a, ref b); swap(ref aIdx, ref bIdx); }

            return bIdx;

        }

        
        private static int partition(int[] array, int start, int end, int pivot) {
            int pivotVal = array[pivot];
            swap(array, pivot, end);
            int storeIdx = start;
            for (int i = start; i < end; i++) {
                if (array[i] < pivotVal) { // array[i].CompareTo(pivotVal) < 0
                    swap(array, i, storeIdx);
                    storeIdx++;
                }
            }
            swap(array, storeIdx, end);
            return storeIdx;
        }
        

        private static void swap(int[] array, int idx1, int idx2) {
            int temp = array[idx1];
            array[idx1] = array[idx2];
            array[idx2] = temp;
        }

        private static void swap(ref int a, ref int b) {
            int temp = a;
            a = b;
            b = temp;
        }
        
    }
}