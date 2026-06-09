using UnityEngine;

/// <summary>
/// 单例类
/// 只要重写 Awake / OnDestroy，都必须调用 base.Awake() / base.OnDestroy()
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T _instance;

    public static T Instance
    {
        get { return _instance; }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }
    protected virtual void OnDestroy()
    {
        if (this as T != _instance)
        {
            return;
        }

        _instance = null;
    }
}