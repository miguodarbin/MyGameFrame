using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 整个计时器的计时单位都是毫秒，1000毫秒 = 1秒
/// </summary>
public class XTimerManager : XSingletonCSharp<XTimerManager>
{
    public static int IdCreator = -1;

    private XTimerManager()
    {
    }

    //管理所有计时器的字典，key是int，value是计时器对象
    private Dictionary<int, XTimerItem> _timerItemsDict = new Dictionary<int, XTimerItem>();

    //待移除的计时器列表容器
    private List<XTimerItem> _needRemoveTimerItemsList = new List<XTimerItem>();


    //============================== 对外公开的接口 ==============================
    //外部主要是通过这个方法，创建一个计时器，并拿到计时器ID。然后传入这个计时器需要执行的回调
    public int CreateTimerItem(int totalDuration, int interval, UnityAction onTimeEndCallback, bool timeScaleAffected = true,
        UnityAction onTimeIntervalCallback = null)
    {
        //从池子里拿出一个干干净净的timerItem
        var timerItem = XPoolManager.Instance.GetCsharpObject<XTimerItem>();

        //然后将这个timerItem初始化成符合外部期望的计时器
        timerItem.InitTimerItem(totalDuration, interval, onTimeEndCallback, onTimeIntervalCallback, timeScaleAffected);

        //然后把这个计时器给个ID并加入到字典里，等着被驱动吧
        int id = ++IdCreator;
        timerItem.ID = id;
        _timerItemsDict.Add(id, timerItem);
        return id;
    }

    public void EnableTimerManager() //开启计时器管理器 
    {
        XMonoManager.Instance.OnUpdateAddListener(LoopCheckTimerItem);
    }

    public void DisableTimerManager() //关闭启计时器管理器 
    {
        XMonoManager.Instance.OnUpdateRemoveListener(LoopCheckTimerItem);
    }

    public void RemoveTimerItem(int id) //移除某个计时器 
    {
        if (!_timerItemsDict.ContainsKey(id))
        {
            Debug.LogError($"没有这个计时器");
            return;
        }

        var timerItem = _timerItemsDict[id];
        _timerItemsDict.Remove(timerItem.ID); //先移掉这次计数器在字典里的记录
        XPoolManager.Instance.ReturnCsharpObject(timerItem); //再归池
    }

    public void ResetTimerItem(int id) //重置某个计时器 
    {
        if (!_timerItemsDict.ContainsKey(id))
        {
            Debug.LogError($"没有这个计时器");
            return;
        }

        _timerItemsDict[id].ResetTimerItem();
    }

    public void ContinueTimerItem(int id) //继续某个计时器 
    {
        if (!_timerItemsDict.ContainsKey(id))
        {
            Debug.LogError($"没有这个计时器");
            return;
        }

        _timerItemsDict[id].IsRunning = true;
    }

    public void StopTimerItem(int id) //暂停某个计时器 
    {
        if (!_timerItemsDict.ContainsKey(id))
        {
            Debug.LogError($"没有这个计时器");
            return;
        }

        _timerItemsDict[id].IsRunning = false;
    }


    //============================== 核心循环方法 ==============================
    //每帧都遍历这个计时器字典看看里面有没有需要计时的去处理.核心计时思想：当累计时间满足了一次interval之后，算一次时间增长，并调一次事件
    private void LoopCheckTimerItem()
    {
        if (_timerItemsDict.Count == 0)
        {
            return;
        }

        //核心遍历逻辑
        foreach (var timerItemPair in _timerItemsDict)
        {
            var timerItem = timerItemPair.Value; //获得计时器对象
            if (!timerItem.IsRunning) //如果计时器不开启就处理下一个
            {
                continue;
            }

            //1.update每帧产生的deltatime用来驱动CurrentInterval和CurrentTime
            float deltaTimeMS;
            if (timerItem.TimeScaleAffected)
            {
                deltaTimeMS = Time.deltaTime * 1000;
            }
            else
            {
                deltaTimeMS = Time.unscaledDeltaTime * 1000;
            }

            timerItem.UpdateTimerItemIntervalTime(deltaTimeMS);
            timerItem.UpdateTimerItemCurrentTime(deltaTimeMS);

            //2.判断是否已经满足了计时器的时间或者间隔时间
            while (timerItem.Interval > 0 && timerItem.CurrentInterval >= timerItem.Interval) //如果满足了间隔时间
            {
                timerItem.OnIntervalEnd();
            }

            if (timerItem.CurrentTime >= timerItem.TotalDuration) //如果满足了计时器的时间
            {
                timerItem.OnTimeEnd();
                _needRemoveTimerItemsList.Add(timerItem);
            }
        }

        //移除计时好的计数器
        if (_needRemoveTimerItemsList.Count > 0)
        {
            foreach (var timerItem in _needRemoveTimerItemsList)
            {
                _timerItemsDict.Remove(timerItem.ID); //先移掉这次计数器在字典里的记录
                XPoolManager.Instance.ReturnCsharpObject(timerItem); //再归池
            }

            _needRemoveTimerItemsList.Clear();
        }
    }
}