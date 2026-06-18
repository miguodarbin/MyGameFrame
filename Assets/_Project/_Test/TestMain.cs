using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniDDOLLoadingTest : MonoBehaviour
{
    public string targetSceneName = "SceneB";

    private Canvas loadingCanvas;
    private Text loadingText;
    private AsyncOperation loadOperation;
    private bool waitMouseClick;

    private void Awake()
    {
        // 这个物体进入 DDOL Scene
        DontDestroyOnLoad(gameObject);

        CreateLoadingUI();
        loadingCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 在 SceneA 按 L 开始切 SceneB
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(LoadScene());
        }

        // Loading 完成后，点击鼠标才真正进入 SceneB
        if (waitMouseClick && Input.GetMouseButtonDown(0))
        {
            waitMouseClick = false;
            loadingText.text = "正在进入场景...";
            loadOperation.allowSceneActivation = true;
        }
    }

    private IEnumerator LoadScene()
    {
        loadingCanvas.gameObject.SetActive(true);
        loadingText.text = "Loading...";

        loadOperation = SceneManager.LoadSceneAsync(targetSceneName);

        // 关键：先不允许场景激活
        loadOperation.allowSceneActivation = false;

        // progress 最大会卡在 0.9，表示“加载完了，但还没切过去”
        while (loadOperation.progress < 0.9f)
        {
            loadingText.text = $"Loading... {loadOperation.progress * 100f:0}%";
            yield return null;
        }

        loadingText.text = "加载完成，点击鼠标进入 SceneB";
        waitMouseClick = true;

        // 等真正切完
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        loadingCanvas.gameObject.SetActive(false);
    }

    private void CreateLoadingUI()
    {
        GameObject canvasObj = new GameObject("DDOL_LoadingCanvas");
        canvasObj.transform.SetParent(transform);

        loadingCanvas = canvasObj.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingCanvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bgObj = new GameObject("BlackBG");
        bgObj.transform.SetParent(canvasObj.transform);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);

        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(bgObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 100);
        textRect.anchoredPosition = Vector2.zero;

        loadingText = textObj.AddComponent<Text>();
        loadingText.alignment = TextAnchor.MiddleCenter;
        loadingText.fontSize = 36;
        loadingText.color = Color.white;
        loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}