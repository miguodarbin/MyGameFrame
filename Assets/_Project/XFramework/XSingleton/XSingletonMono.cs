using UnityEngine;

/// <summary>
/// 给 Mono对象 实现单例
/// </summary>
/// <list type="number">
/// <item>
/// <description>可以给外部一个Mono对象实现单例，但不会自动生成这个Mono单例</description>
/// </item>
/// <item>
/// <description>外部子类如果重写 Awake / OnDestroy，要记得调用 base.Awake() / base.OnDestroy() </description>
/// </item>
/// </list>
public class XSingletonMono<T> : MonoBehaviour where T : XSingletonMono<T>
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