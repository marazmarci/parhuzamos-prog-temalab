using System;
using System.Threading.Tasks;

namespace Feladat3.alg.mergesort {
        
    public class ParallelMergeSort {
    
        public static void Sort(int[] array, int parallelThreshold)
            => parallelMergeSort(array, 0, array.Length - 1, new int[array.Length], parallelThreshold);

        private static void parallelMergeSort(int[] array, int leftStart, int rightEnd, int[] temp, int parallelThreshold) {
            if (rightEnd > leftStart) {
                if (rightEnd - leftStart > parallelThreshold) {
                    int middle = (leftStart + rightEnd) / 2;
                    Parallel.Invoke(
                        () => { parallelMergeSort(array, leftStart, middle, temp, parallelThreshold); },
                        () => { parallelMergeSort(array, middle + 1, rightEnd, temp, parallelThreshold); }
                     );
                    mergeHalves(array, leftStart, rightEnd, temp);
                } else {
                    MergeSort.mergeSort(array, leftStart, rightEnd, temp);
                }
            }
        }
    

        private static void mergeHalves(int[] array, int leftStart, int rightEnd, int[] temp) {
            int leftEnd = (rightEnd + leftStart) / 2;
            int rightStart = leftEnd + 1;
            int size = rightEnd - leftStart + 1;

            int left = leftStart;
            int right = rightStart;
            int i = leftStart;

            while (left <= leftEnd && right <= rightEnd) {
                var arrayLeft = array[left];
                var arrayRight = array[right];
                if (arrayLeft <= arrayRight) {
                    temp[i] = arrayLeft;
                    left++;
                } else {
                    temp[i] = arrayRight;
                    right++;
                }
                i++;
            }
        
            Array.Copy(array, left, temp, i, leftEnd - left + 1);
            Array.Copy(array, right, temp, i, rightEnd - right + 1);
            Array.Copy(temp, leftStart, array, leftStart, size);
        
        }
    
    }
    
    
}