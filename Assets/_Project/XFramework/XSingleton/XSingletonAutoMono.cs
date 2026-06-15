using UnityEngine;

/// <summary>
/// 给 Mono对象 实现单例,并自动生成一个GameObject到场景上
/// </summary>
/// <list type="number">
/// <item>
/// <description>外部子类必须写非 public 的无参构造函数，防止被 new</description>
/// </item>
/// </list>

public class XSingletonAutoMono<T> : MonoBehaviour where T : XSingletonAutoMono<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject haveInstanceObj = new GameObject();
                haveInstanceObj.name = typeof(T).Name;
                haveInstanceObj.AddComponent<T>();
            }

            return _instance;
        }
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