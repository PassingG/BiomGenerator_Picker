using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class CoreSystem : BaseMonoSingleton<CoreSystem>
{
    #region [ Visible Property ]

    [SerializeField]
    private MapData startData;

    [SerializeField]
    private Image mapImage;

    [SerializeField]
    [Header("Generate Count"), Space(10)]
    [Range(0,10)]
    private int generateCount = 1;

    [SerializeField]
    [Header("Smooth Count"), Space(10)]
    [Range(0, 10)]
    private int smoothCount = 1;

    [SerializeField]
    [Header("Displaying interval"), Space(10)]
    [Range(0f, 5f)]
    private float displayingInterval = 1f;
    #endregion


    #region [ Private Property ]

    private BiomGenerator m_BiomGenerator = new BiomGenerator();
    private int[] m_WorldMapData;
    private int2 m_WorldMapSize;

    private WaitForSeconds waitInterval; 

    #endregion

    private bool isDoneInit = false;

    // Call this function when this class is load.
    protected void Start()
    {
        StartCoroutine(Initialize());
        StartCoroutine(SequentialBiomGenerate());
    }

    private IEnumerator Initialize()
    {
        int[,] arrayTmp = startData.GetMapCells();
        m_WorldMapData = Helper.BiomUtility.ConvertToIntArray(arrayTmp);
        m_WorldMapSize = new int2(arrayTmp.GetLength(1), arrayTmp.GetLength(0));

        waitInterval = new WaitForSeconds(displayingInterval);

        isDoneInit = true;
        yield break;
    }

    private IEnumerator SequentialBiomGenerate()
    {
        yield return new WaitUntil(() => isDoneInit == true);

        if (m_BiomGenerator.TryShowMap(mapImage, startData.bioms, m_WorldMapData, m_WorldMapSize).Equals(false))
        {
            Debug.LogError("Cannot displaying biomData");
            yield break;
        }
        
        for (int i = 0; i < generateCount; i++)
        {
            if (m_BiomGenerator.TryBiomGenerate(ref m_WorldMapData, ref m_WorldMapSize).Equals(false))
            {
                Debug.LogError("Cannot generate biomData");
                yield break;
            }

            if (m_BiomGenerator.TryShowMap(mapImage, startData.bioms, m_WorldMapData, m_WorldMapSize).Equals(false))
            {
                Debug.LogError("Cannot displaying biomData");
                yield break;
            }

            yield return waitInterval;
        }

        for (int i = 0; i < smoothCount; i++)
        {
            if (m_BiomGenerator.TrySmoothMap(ref m_WorldMapData, m_WorldMapSize).Equals(false))
            {
                Debug.LogError("Cannot generate biomData");
                yield break;
            }

            if (m_BiomGenerator.TryShowMap(mapImage, startData.bioms, m_WorldMapData, m_WorldMapSize).Equals(false))
            {
                Debug.LogError("Cannot displaying biomData");
                yield break;
            }

            yield return waitInterval;
        }

        yield break;
    }

    private void Release()
    {
        m_WorldMapData = null;
    }

    // Call this function when application is quit.
    private void OnApplicationQuit()
    {
        Release();
    }
}
