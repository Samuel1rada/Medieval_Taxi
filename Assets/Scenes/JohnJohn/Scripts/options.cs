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

    public void SetQuality( int qualitInddex)
    {
        QualitySettings.SetQualityLevel(qualitInddex);
    }

    public void setResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log(resolution);
    }

     //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options2 = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options2.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options2);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();


    }

    public void DropdownResolution()
    {
        string index = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        switch (index)
        {
            case "0":
                Screen.SetResolution(1280, 720, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "1":
                Screen.SetResolution(1920, 1080, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "2":
                Screen.SetResolution(2560, 1440, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "3":
                Screen.SetResolution(3840, 2160, true);
                Debug.Log(Screen.currentResolution);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back()
    {
        options_ui.gameObject.SetActive(false);
    }
}
