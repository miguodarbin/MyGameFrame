using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音效管理器
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> PlayBGM(bgmName)</c>：播放指定背景音乐</description>
/// </item>
/// <item>
///  <description><c> StopBGM()</c>：停止背景音乐</description>
/// </item>
/// <item>
///  <description><c>   ControlBGMPlaying(isPlaying) </c>：控制所有背景音乐播放和暂停</description>
/// </item>
///  <item>
///  <description><c> ChangeBGMVolume(volume) </c>：修改背景音乐音量</description>
/// </item>
///  <item>
///  <description><c> PlaySFX(sfxName, isLoop, callback, isSync) </c>：播放音效，循环音效要提供回调，不循环的不用</description>
/// </item>
///  <item>
///  <description><c>StopSFX(AudioSource sfxAudioSource) </c>：停止音效，一般用于外部停止循环音效</description>
/// </item>
///  <item>
///  <description><c>ChangeSFXVolume(volume) </c>：改变音效的音量 </description>
/// </item>
/// <item>
///  <description><c>ControlAllSFXPlaying( isPlaying) </c>：控制所有音效播放和暂停 </description>
/// </item>
/// /// <item>
///  <description><c>ClearAllSFXPlaying()</c>：过场景或GC的时候用来清理全部音效 </description>
/// </item>
/// <item>
/// <description>资源需要放在 bgm/sfx 包，也就是Assets\Editor\ArtRes\bgm中，外部只传资源名 bgmName</description>
/// </item>
/// <item>
/// <description>可以在播放前先调用一次调整音量，来同步音量数据！</description>
/// </item>
/// /// <item>
/// <description>循环 SFX 时必须传 callback 拿到 AudioSource，外部在合适时机调用 StopSFX 回收</description>
/// </item>
/// </list>
/// </remarks>
public class XAudioManager : XSingletonCSharp<XAudioManager>
{
    //刚开始就让Update里面检测有没有没播放的音效
    private XAudioManager()
    {
        XMonoManager.Instance.OnFixedUpdateAddListener(AutoDestroyFinishedAudioSources);
    }

    private AudioSource _bgmAudioSource = null;
    private float _bgmVolume = 0.5f;

    //播放背景音乐
    public void PlayBGM(string bgmName)
    {
        InitBGMAudioSource();
        XABUnifiedManager.Instance.LoadAsset<AudioClip>("bgm", bgmName, (resultClip) =>
        {
            _bgmAudioSource.clip = resultClip;
            _bgmAudioSource.Play();
        });
    }

    //停止播放
    public void StopBGM()
    {
        if (_bgmAudioSource == null)
            return;
        _bgmAudioSource.Stop();
    }

    //暂停播放
    public void ControlBGMPlaying(bool isPlaying)
    {
        if (_bgmAudioSource == null)
            return;
        if (isPlaying)
        {
            _bgmAudioSource.UnPause();
        }
        else
        {
            _bgmAudioSource.Pause();
        }
    }

    //改变音量
    public void ChangeBGMVolume(float volume)
    {
        InitBGMAudioSource(); //这里也初始化是为了满足外面提前设置音量大小
        _bgmVolume = volume;
        _bgmAudioSource.volume = _bgmVolume;
    }

    //初始化BGMAudioSource
    private void InitBGMAudioSource()
    {
        if (_bgmAudioSource != null)
        {
            return;
        }

        var bgmAudioGameObject = new GameObject("BGMAudioSource");
        Object.DontDestroyOnLoad(bgmAudioGameObject);
        _bgmAudioSource = bgmAudioGameObject.AddComponent<AudioSource>();
        _bgmAudioSource.loop = true;
        _bgmAudioSource.volume = _bgmVolume;
    }

    //__________________下面是SFX相关的__________________________


    //控制SFX的音量
    private float _sfxVolume = 0.5f;

    //持有全部非循环音效的AudioSource
    private List<AudioSource> _sfxNoLoopAudioSources = new List<AudioSource>();
    private List<AudioSource> _sfxLoopAudioSources = new List<AudioSource>();

    //是否自动清理
    private bool _isAutoDestroy = true;


    //播放音效,默认同步加载
    public void PlaySFX(string sfxName, bool isLoop = false, UnityAction<AudioSource> callback = null, bool isSync = true)
    {
        if (isLoop && callback == null)
        {
            Debug.LogError("循环音效需要传入参数为AudioSource的方法，把这个返回给外部管理生命周期");
            return;
        }

        XABUnifiedManager.Instance.LoadAsset<AudioClip>("sfx", sfxName, (resultClip) =>
        {
            var audioSourceObj = XPoolManager.Instance.GetGameObject("AudioSourceObj", true);
            var sfxAudioSource = ResetAudioSource(audioSourceObj.GetComponent<AudioSource>());


            sfxAudioSource.clip = resultClip;
            sfxAudioSource.loop = isLoop;
            sfxAudioSource.volume = _sfxVolume;
            sfxAudioSource.Play();

            //根据是否循环，选择把播放这个Clip的AudioSource交给外部还是自己处理
            if (isLoop) //循环的交给外部处理
            {
                if (!_sfxLoopAudioSources.Contains(sfxAudioSource))
                {
                    _sfxLoopAudioSources.Add(sfxAudioSource);
                }

                callback?.Invoke(sfxAudioSource);
            }
            else //不循环的内部处理
            {
                if (!_sfxNoLoopAudioSources.Contains(sfxAudioSource))
                {
                    _sfxNoLoopAudioSources.Add(sfxAudioSource);
                }
            }
        }, isSync);
    }

    //停止音效
    public void StopSFX(AudioSource sfxAudioSource)
    {
        if (sfxAudioSource == null)
        {
            return;
        }

        XPoolManager.Instance.ReturnGameObject(ResetAudioSource(sfxAudioSource).gameObject);
    }

    //改变全部音效的声音大小
    public void ChangeSFXVolume(float volume)
    {
        _sfxVolume = volume;
        foreach (var audioSource in _sfxLoopAudioSources)
        {
            audioSource.volume = _sfxVolume;
        }

        foreach (var audioSource in _sfxNoLoopAudioSources)
        {
            audioSource.volume = _sfxVolume;
        }
    }

    //控制所有音效暂停或者继续播放
    public void ControlAllSFXPlaying(bool isPlaying)
    {
        if (isPlaying)
        {
            _isAutoDestroy = true;
            foreach (var audioSource in _sfxLoopAudioSources)
            {
                audioSource.UnPause();
            }

            foreach (var audioSource in _sfxNoLoopAudioSources)
            {
                audioSource.UnPause();
            }
        }
        else //如果只是Pause的话，也希望自动清理暂时失效，所以要多加一个自动清理开关
        {
            _isAutoDestroy = false;
            foreach (var audioSource in _sfxLoopAudioSources)
            {
                audioSource.Pause();
            }

            foreach (var audioSource in _sfxNoLoopAudioSources)
            {
                audioSource.Pause();
            }
        }
    }

    //清除列表全部引用,停止音效并回池
    public void ClearAllSFXPlaying()
    {
        for (int i = _sfxLoopAudioSources.Count - 1; i >= 0; i--)
        {
            var audioSource = ResetAudioSource(_sfxLoopAudioSources[i]);
            XPoolManager.Instance.ReturnGameObject(audioSource.gameObject);
        }

        for (int i = _sfxNoLoopAudioSources.Count - 1; i >= 0; i--)
        {
            var audioSource = ResetAudioSource(_sfxNoLoopAudioSources[i]);
            XPoolManager.Instance.ReturnGameObject(audioSource.gameObject);
        }
    }


    /// <summary>
    /// 停止所有SFX，清空clip，从管理列表移除，并把AudioSourceObj归还对象池
    /// </summary>
    private void AutoDestroyFinishedAudioSources()
    {
        if (!_isAutoDestroy)
        {
            return;
        }

        if (_sfxNoLoopAudioSources.Count == 0)
        {
            return;
        }

        for (int i = _sfxNoLoopAudioSources.Count - 1; i >= 0; i--)
        {
            if (!_sfxNoLoopAudioSources[i].isPlaying)
            {
                XPoolManager.Instance.ReturnGameObject(ResetAudioSource(_sfxNoLoopAudioSources[i]).gameObject);
            }
        }
    }


    //重置AudioSource，并从SFX管理列表中移除
    private AudioSource ResetAudioSource(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
        _sfxNoLoopAudioSources.Remove(audioSource);
        _sfxLoopAudioSources.Remove(audioSource);
        return audioSource;
    }
}