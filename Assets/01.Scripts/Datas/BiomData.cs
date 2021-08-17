using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomData", menuName = "Datas/BiomData", order = 0)]
public class BiomData : ScriptableObject
{
    [SerializeField]
    private string m_label;

    [SerializeField]
    private Color m_BiomColor = Color.white;
    public Color biomColor => m_BiomColor;
}