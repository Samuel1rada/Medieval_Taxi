using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Slider = UnityEngine.UI.Slider;

public class Options : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public AudioMixer audioMixer;
    public Canvas options_ui;

    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider SFXSlider;
    [SerializeField] public Toggle fullscreen;

    // 🆕 ADD THIS: first selected UI element when opening options
    [SerializeField] private GameObject firstSelected;

    void Start()
    {
        if (PlayerPrefs.HasKey("full"))
            loadFullscreen();
        else
            Setfullscreen();

        if (PlayerPrefs.HasKey("volumeMusic"))
            LoadVolume();
        else
        {
            SetVolume();
            SetVolumeSfx();
        }
    }

    public void SetVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("volume_music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("volumeMusic", volume);
    }

    public void SetVolumeSfx()
    {
        float volume = SFXSlider.value;
        audioMixer.SetFloat("volume_sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("volumeSFX", volume);
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

    public void Setfullscreen()
    {
        bool isFull = fullscreen.isOn;
        PlayerPrefs.SetInt("full", (isFull ? 1 : 0));
        Screen.fullScreen = isFull;
        Debug.Log(isFull ? "Fullscreen!" : "Not Fullscreen!");
    }

    private void loadFullscreen()
    {
        if (fullscreen != null)
        {
            fullscreen.isOn = (PlayerPrefs.GetInt("full") != 0);
        }

        Setfullscreen();
    }

    // 🆕 CALL THIS WHEN OPENING OPTIONS MENU
    public void OpenOptions()
    {
        options_ui.gameObject.SetActive(true);

        // Reset then select first UI element for controller
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void Back()
    {
        options_ui.gameObject.SetActive(false);

        // 🆕 Reset selection when closing menu
        EventSystem.current.SetSelectedGameObject(null);
    }
}
