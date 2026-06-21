using UnityEngine.Events;

public class XTimerItem : IXPoolObject
{
    public int ID { get; set; } //唯一ID
    public bool IsRunning { get; set; } = true; //是否启用中
    public bool TimeScaleAffected { get; private set; } = true; //是否受TimeScale影响

    private UnityAction _onTimeEndCallback; //当计时器结束时调用的回调
    private UnityAction _onTimeIntervalCallback; //当计时器在每一次增加时间间隔的时候调用的回调

    public int TotalDuration { get; private set; } //外部希望的这个计时器的总计时 时长
    public int Interval { get; private set; } // 外部希望的这个计时器的间隔多久算一次增长时间

    public int CurrentInterval { get; private set; } //核心计时思想：当累计时间满足了一次interval之后，算一次时间增长，并调一次事件

    public int CurrentTime { get; private set; } = 0; //当前计时器走的时间


    //============================== 对外公开的接口 ==============================
    //这个方法用来让TimerManager初始化计时器的
    public void InitTimerItem(int totalDuration, int interval, UnityAction onTimeEndCallback,
        UnityAction onTimeIntervalCallback = null, bool timeScaleAffected = true)
    {
        _onTimeEndCallback = onTimeEndCallback;
        _onTimeIntervalCallback = onTimeIntervalCallback;
        TotalDuration = totalDuration;
        Interval = interval;
        TimeScaleAffected = timeScaleAffected;
    }

    //这个方法用来增加计时器的CurrentInterval
    public void UpdateTimerItemIntervalTime(int deltaTime)
    {
        CurrentInterval += deltaTime;
    }

    //这个方法是真正用来增加计时器CurrentTime的
    public void UpdateTimerItemCurrentTime(int deltaTime)
    {
        CurrentTime += deltaTime;
    }

    //重置一个填好信息的计时器
    public void ResetTimerItem()
    {
        IsRunning = true;
        CurrentInterval = 0;
        CurrentTime = 0;
    }

    //============================== 辅助方法 ==============================
    public void OnTimeEnd() //给外部触发计时结束回调的方法
    {
        _onTimeEndCallback?.Invoke(); //调用计时器时间结束的回调
    }

    public void OnIntervalEnd() //给外部触发满足一次interval回调的方法
    {
        CurrentInterval -= Interval; //重置 CurrentInterval，多的部分给下一个周期
        _onTimeIntervalCallback?.Invoke(); //调用Interval的回调
    }


    //============================== 必须实现的接口方法 ==============================
    public void OnGetFromPool() //取出时重置的方法，不具体赋值，只是让他变成干净可用的
    {
        CurrentInterval = 0;
        CurrentTime = 0;
        IsRunning = true;
    }

    public void OnReturnToPool() //清理旧数据，让他干干净净回池子
    {
        ID = 0;
        IsRunning = false;
        TimeScaleAffected = true;
        _onTimeEndCallback = null;
        _onTimeIntervalCallback = null;
        TotalDuration = 0;
        Interval = 0;
        CurrentInterval = 0;
        CurrentTime = 0;
    }
}