using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM背景音乐管理器
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
///  <description><c>  PauseBGM() </c>：暂停背景音乐</description>
/// </item>
/// /// <item>
///  <description><c> ChangeVolume(volume) </c>：修改背景音乐音量</description>
/// </item>
/// <item>
/// <description>资源需要放在 bgm 包，也就是Assets\Editor\ArtRes\bgm中，外部只传资源名 bgmName</description>
/// </item>
/// </list>
/// </remarks>
public class XBGMManager : XSingletonCSharp<XBGMManager>
{
    private XBGMManager()
    {
    }

    private AudioSource _bgmAudioSource = null;
    private float _volume = 0.5f;

    //播放背景音乐
    public void PlayBGM(string bgmName)
    {
        InitAudioSource();
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
    public void PauseBGM()
    {
        if (_bgmAudioSource == null)
            return;
        _bgmAudioSource.Pause();
    }

    //改变音量
    public void ChangeVolume(float volume)
    {
        InitAudioSource();
        _volume = volume;
        _bgmAudioSource.volume = _volume;
    }

    //初始化AudioSource
    private void InitAudioSource()
    {
        if (_bgmAudioSource != null)
        {
            return;
        }

        var bgmAudioGameObject = new GameObject("BGMAudioSource");
        Object.DontDestroyOnLoad(bgmAudioGameObject);
        _bgmAudioSource = bgmAudioGameObject.AddComponent<AudioSource>();
        _bgmAudioSource.loop = true;
        _bgmAudioSource.volume = _volume;
    }
}