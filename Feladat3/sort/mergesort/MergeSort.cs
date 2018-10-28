using System;
using System.Linq;

namespace Feladat3.alg.mergesort {
    
    public class MergeSort {
        
        public static void Sort(int[] array) // TODO remove notParallelThreshold from single thread impl
            => mergeSort(array, 0, array.Length - 1, new int[array.Length]);

        internal static void mergeSort(int[] array, int leftStart, int rightEnd, int[] temp) {
            if (rightEnd > leftStart) {
                int middle = (leftStart + rightEnd) / 2;
                mergeSort(array, leftStart, middle, temp);
                mergeSort(array, middle + 1, rightEnd, temp);
                mergeHalves(array, leftStart, rightEnd, temp);
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