using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreSystem : MonoBehaviour
{
    // Call this function when this class is load.
    private void Awake()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        BiomGenerator.Instance.Initialize();

        yield break;
    }

    private void Release()
    {
        BiomGenerator.DestroyInstance();
    }

    // Call this function when application is quit.
    private void OnApplicationQuit()
    {
        Release();
    }
}
