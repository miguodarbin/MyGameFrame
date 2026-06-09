using UnityEngine;
/// <summary>
/// 实现自动访问属性自动生成对象并挂载单例组件的类
/// 只要重写 Awake / OnDestroy，都必须调用 base.Awake() / base.OnDestroy()
/// </summary>
/// <typeparam name="T"></typeparam>

public class SingletonAutoMono<T> : MonoBehaviour where T : SingletonAutoMono<T>
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