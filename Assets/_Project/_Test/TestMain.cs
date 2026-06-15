using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMain : MonoBehaviour
{
    public Image image;
    public Button playMusicAButton;
    public Button playMusicBButton;

    public Button stopPlayButton;
    public Button pausePlayButton;

    public Slider musicVolume;

    public Button playSFXButton;

    public Button pauseSFXButton;

    private AudioSource _loopAudioSource;

    private bool _isPlaying = true;

    private void Start()
    {
        XAudioManager.Instance.ChangeBGMVolume(musicVolume.value);
        XAudioManager.Instance.ChangeSFXVolume(musicVolume.value);
        XABUnifiedManager.Instance.LoadAsset<Sprite>("ui", "100", (arg => image.sprite = arg));

        playMusicAButton.onClick.AddListener(() => { XAudioManager.Instance.PlayBGM("AMusic"); }
        );

        playMusicBButton.onClick.AddListener(() => { XAudioManager.Instance.PlayBGM("BMusic"); }
        );

        stopPlayButton.onClick.AddListener(() => { XAudioManager.Instance.StopBGM(); });
        musicVolume.onValueChanged.AddListener(value =>
            {
                XAudioManager.Instance.ChangeBGMVolume(value);
                XAudioManager.Instance.ChangeSFXVolume(value);
            }
        );

        //playSFXButton.onClick.AddListener(() => { XAudioMManager.Instance.PlaySFX("a", true, (resultAudioSource => _loopAudioSource = resultAudioSource)); });
        playSFXButton.onClick.AddListener(() => { XAudioManager.Instance.PlaySFX("b"); });

        pauseSFXButton.onClick.AddListener(() =>
        {
            _isPlaying = !_isPlaying;
            XAudioManager.Instance.ControlAllSFXPlaying(_isPlaying);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            XAudioManager.Instance.StopSFX(_loopAudioSource);
        }
    }
}