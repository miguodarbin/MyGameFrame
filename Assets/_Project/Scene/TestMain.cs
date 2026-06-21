using UnityEngine;


public class TestMain : MonoBehaviour
{
    void Start()
    {
        XTimerManager.Instance.EnableTimerManager();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(Time.time);
            XTimerManager.Instance.CreateTimerItem(4000, 500, () => { Debug.Log("释放火球" + Time.time); }, false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Time.timeScale = 0;
        }
    }
}