using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreSystem : BaseMonoSingleton<CoreSystem>
{
    // Call this function when this class is load.
    protected override void Awake()
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
        BiomGenerator.Instance.Release();
    }

    // Call this function when application is quit.
    private void OnApplicationQuit()
    {
        Release();
    }
}
