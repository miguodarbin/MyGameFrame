using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    private void Awake()
    {
        // 关闭 VSync，让 targetFrameRate 生效
        QualitySettings.vSyncCount = 0;

        // 限制 Unity 运行帧率
        Application.targetFrameRate = 60;
    }
}