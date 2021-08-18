using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Helper
{
    public class BiomUtility
    {
        public static int[] ConvertToIntArray(int[,] array)
        {
            int2 arraySize = new int2(array.GetLength(1), array.GetLength(0));
            int[] result = new int[arraySize.x * arraySize.y];

            for (int y = 0; y < arraySize.y; y++)
            {
                for (int x = 0; x < arraySize.x; x++)
                {
                    int arrayIndex = x + (arraySize.x * y);
                    result[arrayIndex] = array[y,x];
                }
            }

            return result;
        }
    }
}

