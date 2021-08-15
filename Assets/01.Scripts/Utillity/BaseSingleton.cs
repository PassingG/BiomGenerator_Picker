using UnityEngine;

public interface IBaseSingleton
{
    void OnCreateInstance();
    void OnDestroyInstance();
}

public abstract class BaseSingleton<T> where T : class, IBaseSingleton, new()
{
    private static T instance;
    public static T Instance => instance;

    public static T CreateInstance()
    {
        if(instance == null)
        {
            instance = new T();
            instance.OnCreateInstance();
        }
        return instance;
    }

    public static void DestroyInstance()
    {
        Instance?.OnDestroyInstance();
        instance = null;
    } 
}

public abstract class BaseMonoSingleton<T> : MonoBehaviour where T : class
{
    public static T Instance { get; private set; }

    [SerializeField]
    private bool m_IsDontDestroyObject = false;
    public bool isDontDestroyObject => m_IsDontDestroyObject;

    public static T1 GetInstance<T1>() where T1 : class, T
    {
        return Instance as T1;
    }

    protected virtual void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this as T;
            if (isDontDestroyObject) DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if(Instance.Equals(this))
        {
            Instance = null;
        }
    }
}