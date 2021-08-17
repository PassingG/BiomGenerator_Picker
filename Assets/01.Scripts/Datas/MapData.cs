using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Datas/MapData", order = 0)]
public class MapData : ScriptableObject
{
    /// <summary>
    /// Scriptable Objects cannot display two dimensions. 
    /// So if you put a value array in the Row Class and have the class as an array, 
    /// the one-dimensional array appears to have a class, but the class has another array, which can have a two-dimensional effect.
    /// </summary>
    [System.Serializable]
    public class CellRow
    {
        [SerializeField]
        private int[] row = new int[MapData.defaultGridSize];

        public int this[int i] => row[i];
    }
    
    // Default GridSize
    public const int defaultGridSize = 3;

    #region [ Property ]
    [SerializeField]
    private CellRow[] m_MapData = new CellRow[defaultGridSize];
    public CellRow[] mapData => m_MapData;

    [SerializeField]
    private Vector2Int m_MapGridSize = Vector2Int.one * defaultGridSize;
    public Vector2Int mapGridSize => m_MapGridSize;

    [SerializeField]
    private BiomData[] m_Bioms;
    public BiomData[] bioms => m_Bioms;
    #endregion

    public int[,] GetMapCells()
    {
        int[,] ret = new int[mapGridSize.x, mapGridSize.y];

        for (var y = 0; y < mapGridSize.y; y++)
        {
            for (var x = 0; x < mapGridSize.x; x++)
            {
                ret[x, y] = GetMapRow(y)[x];
            }
        }

        return ret;
    }

    public CellRow GetMapRow(int idx)
    {
        return mapData[idx];
    }
}