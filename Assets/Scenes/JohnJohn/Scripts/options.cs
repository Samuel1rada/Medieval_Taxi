using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class Options : MonoBehaviour
{

    [SerializeField] private TMP_Text text;

    public AudioMixer audioMixer;

    public AudioMixer audioMixerSfx;

    public Dropdown resolutionDropdown;

    public Canvas options_ui;

    Resolution[] resolutions;


    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume_music", volume);
        //Debug.Log(volume);
    }

    public void SetVolumeSfx(float volume)
    {
        audioMixer.SetFloat("volume_sfx", volume);
        //Debug.Log(volume);
    }



     //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {



    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setfullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        

        if (Screen.fullScreen = isFullscreen)
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
