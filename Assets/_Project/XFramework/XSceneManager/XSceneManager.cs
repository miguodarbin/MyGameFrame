using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
            XEventCenter.Instance.EventTrigger<float>(XEventType.E_SceneLoadProgress, request.progress);
            callback?.Invoke(request);
            yield return null;
        }

        XEventCenter.Instance.EventTrigger<float>(XEventType.E_SceneLoadProgress, 1);

        XEventCenter.Instance.EventTrigger<AsyncOperation>(XEventType.E_SceneLoadSucess, request);
        callback?.Invoke(request);
    }
}