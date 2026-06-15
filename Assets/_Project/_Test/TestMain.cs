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

    private void Start()
    {
        XBGMManager.Instance.ChangeVolume(musicVolume.value);
        XABUnifiedManager.Instance.LoadAsset<Sprite>("ui", "100", (arg => image.sprite = arg));

        playMusicAButton.onClick.AddListener(() => { XBGMManager.Instance.PlayBGM("AMusic"); }
        );

        playMusicBButton.onClick.AddListener(() => { XBGMManager.Instance.PlayBGM("BMusic"); }
        );

        stopPlayButton.onClick.AddListener(() => { XBGMManager.Instance.StopBGM(); });
        pausePlayButton.onClick.AddListener(() => { XBGMManager.Instance.PauseBGM(); });
        musicVolume.onValueChanged.AddListener(value => { XBGMManager.Instance.ChangeVolume(value); }
        );
    }
}