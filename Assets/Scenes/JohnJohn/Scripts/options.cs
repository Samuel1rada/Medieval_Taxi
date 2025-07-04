using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using Slider = UnityEngine.UI.Slider;

public class Options : MonoBehaviour
{

    [SerializeField] private TMP_Text text;

    public AudioMixer audioMixer;

    public Canvas options_ui;

    [SerializeField] public Slider musicSlider;

    [SerializeField] public Slider SFXSlider;




    void Start()
    {

        if (PlayerPrefs.HasKey("volumeMusic"))
        {
            LoadVolume();
        }
        else
        {
            SetVolume();
            SetVolumeSfx();
        }

    }

    public void SetVolume()
    {
        float volume = musicSlider.value;

        audioMixer.SetFloat("volume_music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("volumeMusic", volume);
        //Debug.Log(volume);
    }

    public void SetVolumeSfx()
    {
        float volume = SFXSlider.value;

        audioMixer.SetFloat("volume_sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("volumeSFX", volume);
        //Debug.Log(volume);
    }

    public void LoadVolume()
    {
        if (audioMixer != null && musicSlider != null && SFXSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("volumeMusic");
            SFXSlider.value = PlayerPrefs.GetFloat("volumeSFX");

            SetVolume();
            SetVolumeSfx();
        }
        else
        {
            Debug.LogError("audioMixer, musicSlider, or SFXSlider is null in LoadVolume()");
        }
    }




    //Start is called once before the first execution of Update after the MonoBehaviour is created




    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setfullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        

        if (Screen.fullScreen == isFullscreen)
        {
            Debug.Log("Fullscreen!");
        }
        else
        {
            Debug.Log("Not Fullscreen!");
        }

    }

    public void Back()
    {
        options_ui.gameObject.SetActive(false);
    }
}
