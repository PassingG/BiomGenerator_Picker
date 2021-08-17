using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.UI;

public class BiomGenerator : BaseMonoSingleton<BiomGenerator>
{
    #region [ Function Class ]

    private BiomDisplay biomDisplay;

    #endregion


    #region [ Visible Property ]

    [SerializeField]
    private MapData startData;
    
    [SerializeField]
    private Image mapImage;

    #endregion


    #region [ Private Property ]



    #endregion

    public void Initialize()
    {
        biomDisplay.DisplayMap(mapImage, startData.GetMapCells());
    }
    
    public void FirstBiomCreate()
    {

    }

    public void Release()
    {

    }
}
