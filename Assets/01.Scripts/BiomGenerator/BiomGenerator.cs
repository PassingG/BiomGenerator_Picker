using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.UI;

public class BiomGenerator
{
    public bool TryBiomGenerate(ref int[] mapdata, ref int2 mapsize)
    {
        int mapListLength = mapdata.Length;

        NativeArray<int> tmpMapArray = new NativeArray<int>(mapListLength, Allocator.TempJob);

        tmpMapArray.CopyFrom(mapdata);

        // Calculate size after expansion.
        int2 gridSize = mapsize;
        gridSize = gridSize * 2 - 1;

        // Expansion Data
        NativeArray<int> currentNativeArray = new NativeArray<int>(gridSize.x * gridSize.y, Allocator.TempJob);
        
        ExpansionGridJob expansionGridJob = new ExpansionGridJob
        {
            currentNativeArray = currentNativeArray,
            gridSizeX = gridSize.x,
            mapList = tmpMapArray
        };
        JobHandle expansionJobHandle = expansionGridJob.Schedule(gridSize.x * gridSize.y, 64);

        expansionJobHandle.Complete();


        // Fill the random data at the blank area.
        NativeArray<int> currentNativeArrayTmp = new NativeArray<int>(gridSize.x * gridSize.y, Allocator.TempJob);

        currentNativeArrayTmp.CopyFrom(currentNativeArray);

        uint randSeed = (uint)UnityEngine.Random.Range(1, 9999);

        Unity.Mathematics.Random random = new Unity.Mathematics.Random(randSeed);

        CalculateBlankGridJob calculateBlankGridJob = new CalculateBlankGridJob
        {
            currentNativeArray = currentNativeArray,
            currentNativeArrayTmp = currentNativeArrayTmp,
            gridSizeX = gridSize.x,
            random = random
        };
        JobHandle calculateBlankGridJobHandle = calculateBlankGridJob.Schedule(gridSize.x * gridSize.y, 64);

        calculateBlankGridJobHandle.Complete();

        // Copy to Ref source.
        mapdata = currentNativeArray.ToArray();
        mapsize = gridSize;
        // Dispose Memory
        currentNativeArray.Dispose();
        tmpMapArray.Dispose();
        currentNativeArrayTmp.Dispose();

        return true;
    }

    #region [ Function ]
    public bool TryShowMap(Image mapImage, BiomData[] bioms, int[] mapData, int2 mapSize)
    {
        // Make map image's texture. this size must have same mapData's length.
        Texture2D mapTexture = new Texture2D(mapSize.x, mapSize.y);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] colorTmp = new Color[mapSize.x * mapSize.y];

        // Set mapData Texture
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                int arrayIndex = x + (mapSize.x * y);
                int biomData = mapData[arrayIndex];

                if (biomData > bioms.Length || biomData < 0)
                {
                    return false;
                }

                colorTmp[arrayIndex] = bioms[biomData].biomColor;
            }
        }

        mapTexture.SetPixels(0, 0, mapSize.x, mapSize.y, colorTmp);
        mapTexture.Apply();

        // Make Sprite
        Rect rect = new Rect(0, 0, mapSize.x, mapSize.y);
        Sprite mapSprite = Sprite.Create(mapTexture, rect, new Vector2(0.5f, 0.5f));

        // Set Image
        mapImage.sprite = mapSprite;

        return true;
    }
    public bool TrySmoothMap(ref int[] mapdata, int2 mapsize)
    {
        int mapListLength = mapdata.Length;
    
        NativeArray<int> tmpMapArray = new NativeArray<int>(mapListLength, Allocator.TempJob);

        tmpMapArray.CopyFrom(mapdata);

        NativeArray<int> currentMapArray = new NativeArray<int>(mapListLength, Allocator.TempJob);

        currentMapArray.CopyFrom(mapdata);

        uint randSeed = (uint)UnityEngine.Random.Range(1, 9999);

        Unity.Mathematics.Random random = new Unity.Mathematics.Random(randSeed);

        SmoothDataJob smoothDataJob = new SmoothDataJob
        {
            current = currentMapArray,
            tmpMapList = tmpMapArray,
            Length = mapsize,
            random = random
        };
        JobHandle smoothDataJobHandle = smoothDataJob.Schedule(mapListLength, 64);

        smoothDataJobHandle.Complete();

        mapdata = currentMapArray.ToArray();

        currentMapArray.Dispose();
        tmpMapArray.Dispose();

        return true;
    }
    #endregion

    #region [ Jobs ]
    [BurstCompile]
    public struct SmoothDataJob : IJobParallelFor
    {
        [ReadOnly]
        public int2 Length;

        [ReadOnly]
        public NativeArray<int> tmpMapList;

        [WriteOnly]
        public NativeArray<int> current;

        [ReadOnly]
        public Unity.Mathematics.Random random;

        public void Execute(int index)
        {
            int x = index % Length.x;
            int y = (int)math.floor(index / Length.x);

            // It cannot calculate edge
            if (x != 0 && y != 0 && x < Length.x - 1 && y < Length.y - 1)
            {
                if (tmpMapList[(x - 1) + (y * Length.x)] == tmpMapList[(x + 1) + (y * Length.x)] && tmpMapList[x + ((y - 1) * Length.x)] == tmpMapList[x + ((y + 1) * Length.x)])
                {
                    int indexX;
                    int indexY;
                    if (random.NextInt(0, 2) == 0)
                    {
                        indexX = x - 1;
                        indexY = y;
                    }
                    else
                    {
                        indexX = x;
                        indexY = y - 1;
                    }

                    current[x + (y * Length.x)] = tmpMapList[indexX + (indexY * Length.x)];
                }
                else if (tmpMapList[(x - 1) + (y * Length.x)] == tmpMapList[(x + 1) + (y * Length.x)] && tmpMapList[x + ((y - 1) * Length.x)] != tmpMapList[x + ((y + 1) * Length.x)])
                {
                    int indexX = x - 1;

                    current[x + (y * Length.x)] = tmpMapList[indexX + (y * Length.x)];
                }
                else if (tmpMapList[(x - 1) + (y * Length.x)] != tmpMapList[(x + 1) + (y * Length.x)] && tmpMapList[x + ((y - 1) * Length.x)] == tmpMapList[x + ((y + 1) * Length.x)])
                {
                    int indexY = y - 1;

                    current[x + (y * Length.x)] = tmpMapList[x + (indexY * Length.x)];
                }
            }
        }
    }

    [BurstCompile]
    public struct ExpansionGridJob : IJobParallelFor
    {
        [ReadOnly]
        public int gridSizeX;

        [WriteOnly]
        public NativeArray<int> currentNativeArray;

        [ReadOnly]
        public NativeArray<int> mapList;

        public void Execute(int index)
        {
            int Xpos = index % gridSizeX;
            int Ypos = (int)math.floor(index / gridSizeX);

            if ((float)Xpos % 2f == 0f && Ypos % 2f == 0f)
            {
                currentNativeArray[index] = mapList[(Xpos / 2) + ((Ypos / 2) * ((gridSizeX + 1) / 2))];
            }
            else
            {
                currentNativeArray[Xpos + (Ypos * gridSizeX)] = -1;
            }
        }
    }

    [BurstCompile]
    public struct CalculateBlankGridJob : IJobParallelFor
    {
        [ReadOnly]
        public int gridSizeX;

        [WriteOnly]
        public NativeArray<int> currentNativeArray;

        [ReadOnly]
        public NativeArray<int> currentNativeArrayTmp;

        [ReadOnly]
        public Unity.Mathematics.Random random;

        public void Execute(int index)
        {
            int x = index % gridSizeX;
            int y = (int)Unity.Mathematics.math.floor(index / gridSizeX);

            if ((float)x % 2f == 1f && (float)y % 2f == 0f)
            {
                int indexX;

                if (random.NextInt(0, 2) == 0)
                {
                    indexX = x - 1;

                    //if (current[indexX, y] == 5)
                    //{
                    //    indexX = x + 1;
                    //}
                }
                else
                {
                    indexX = x + 1;

                    //if (current[indexX, y] == 5)
                    //{
                    //    indexX = x - 1;
                    //}
                }

                currentNativeArray[index] = currentNativeArrayTmp[indexX + (gridSizeX * y)];
            }
            else if ((float)x % 2f == 0f && (float)y % 2f == 1f)
            {
                int indexY;


                if (random.NextInt(0, 2) == 0)
                {
                    indexY = y - 1;

                    //if (current[x, indexY] == 5)
                    //{
                    //    indexY = y + 1;
                    //}
                }
                else
                {
                    indexY = y + 1;

                    //if (current[x, indexY] == 5)
                    //{
                    //    indexY = y - 1;
                    //}
                }
                currentNativeArray[index] = currentNativeArrayTmp[x + (gridSizeX * indexY)];
            }
            else if ((float)x % 2f == 1f && (float)y % 2f == 1f)
            {
                int indexX;
                int indexY;

                switch (random.NextInt(0, 4))
                {
                    case 0:
                        indexX = x - 1;
                        indexY = y - 1;
                        break;
                    case 1:
                        indexX = x + 1;
                        indexY = y - 1;
                        break;
                    case 2:
                        indexX = x - 1;
                        indexY = y + 1;
                        break;
                    case 3:
                        indexX = x + 1;
                        indexY = y + 1;
                        break;
                    default:
                        indexX = x;
                        indexY = y;
                        break;
                }

                currentNativeArray[index] = currentNativeArrayTmp[indexX + (gridSizeX * indexY)];
            }
        }
    }
    #endregion
}
