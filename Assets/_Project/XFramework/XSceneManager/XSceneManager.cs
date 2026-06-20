using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
/// <summary>
/// 【 场景加载管理器 】
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> LoadScene(sceneName, callback) </c>：同步切换场景，加载完成后执行 callback  </description>
/// </item>
/// <item>
/// <description><c> LoadSceneAsync(sceneName, callback) </c>：异步加载场景，callback 会持续返回 AsyncOperation，外部可读取 progress  </description>
/// </item>
/// <item>
/// <description><c> 异步加载不会自动切换场景，需要手动设置 request.allowSceneActivation = true，才会进入新场景 </c>：</description>
/// </item>
/// </list>
/// </remarks>
public class XSceneManager : XSingletonCSharp<XSceneManager>
{
    private XSceneManager()
    {
    }

    public void LoadScene(string sceneName, UnityAction callback)
    {
        SceneManager.LoadScene(sceneName);
        callback?.Invoke();
    }

    public void LoadSceneAsync(string sceneName, UnityAction<AsyncOperation> callback)
    {
        XMonoManager.Instance.StartCoroutine(ReallyLoadScene(sceneName, callback));
    }

    public IEnumerator ReallyLoadScene(string sceneName, UnityAction<AsyncOperation> callback)
    {
        var request = SceneManager.LoadSceneAsync(sceneName);
        request.allowSceneActivation = false;
        while (request.progress < 0.9f)
        {
            callback?.Invoke(request);
            yield return null;
        }
        
        callback?.Invoke(request);
    }
}