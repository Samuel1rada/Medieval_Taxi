//using UnityEngine;

//public class SoundManager : MonoBehaviour
//{
//    public static SoundManager Instance { get; private set; }

//    [Header("Audio Sources")]
//    [SerializeField] private AudioSource musicSource;
//    [SerializeField] private AudioSource ambientSource;
//    [SerializeField] private AudioSource sfxSource;
//    [SerializeField] private AudioSource movementSource;

//    [Header("Music Settings")]
//    [Tooltip("Background music clip")]
//    [SerializeField] private SoundData backgroundMusic;

//    [Header("Ambient Settings")]
//    [Tooltip("Current ambient sound")]
//    [SerializeField] private SoundData ambientSound;

//    [Header("Player Movement Sounds")]
//    [Tooltip("Sound played during player movement")]
//    [SerializeField] private SoundData movementSound;

//    [Header("General SFX")]
//    [Tooltip("Array of general sound effects")]
//    [SerializeField] private SoundData[] sfxClips;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        PlayBackgroundMusic();
//        PlayAmbientSound();
//    }

//    #region Music
//    public void PlayBackgroundMusic()
//    {
//        if (backgroundMusic == null || musicSource == null) return;
//        musicSource.clip = backgroundMusic.clip;
//        musicSource.volume = backgroundMusic.volume;
//        musicSource.pitch = backgroundMusic.pitch;
//        musicSource.loop = backgroundMusic.loop;
//        musicSource.Play();
//    }
//    #endregion

//    #region Ambient
//    public void PlayAmbientSound()
//    {
//        if (ambientSound == null || ambientSource == null) return;
//        ambientSource.clip = ambientSound.clip;
//        ambientSource.volume = ambientSound.volume;
//        ambientSource.pitch = ambientSound.pitch;
//        ambientSource.loop = ambientSound.loop;
//        ambientSource.Play();
//    }

//    public void TransitionAmbient(SoundData newAmbient, float transitionTime = 1f)
//    {
//        StartCoroutine(FadeAmbient(newAmbient, transitionTime));
//    }

//    private System.Collections.IEnumerator FadeAmbient(SoundData newAmbient, float time)
//    {
//        float startVolume = ambientSource.volume;
//        float timer = 0f;

//        while (timer < time)
//        {
//            ambientSource.volume = Mathf.Lerp(startVolume, 0, timer / time);
//            timer += Time.deltaTime;
//            yield return null;
//        }

//        ambientSource.clip = newAmbient.clip;
//        ambientSource.volume = 0;
//        ambientSource.Play();

//        timer = 0f;
//        while (timer < time)
//        {
//            ambientSource.volume = Mathf.Lerp(0, newAmbient.volume, timer / time);
//            timer += Time.deltaTime;
//            yield return null;
//        }
//        ambientSource.volume = newAmbient.volume;
//    }
//    #endregion

//    #region Movement
//    public void PlayMovementSound()
//    {
//        if (movementSound == null || movementSource == null || movementSource.isPlaying) return;
//        movementSource.clip = movementSound.clip;
//        movementSource.volume = movementSound.volume;
//        movementSource.pitch = movementSound.pitch;
//        movementSource.loop = movementSound.loop;
//        movementSource.Play();
//    }

//    public void StopMovementSound()
//    {
//        if (movementSource.isPlaying)
//            movementSource.Stop();
//    }
//    #endregion

//    #region SFX
//    public void PlaySFX(string name)
//    {
//        SoundData sfx = System.Array.Find(sfxClips, s => s.soundName == name);
//        if (sfx != null && sfxSource != null)
//        {
//            sfxSource.PlayOneShot(sfx.clip, sfx.volume);
//        }
//        else
//        {
//            Debug.LogWarning("SFX not found: " + name);
//        }
//    }
//    #endregion
//}

/*

// Play an SFX
SoundManager.Instance.PlaySFX("Jump");

// Transition ambient
SoundManager.Instance.TransitionAmbient(newAmbientSound, 2f);

// Player starts moving
SoundManager.Instance.PlayMovementSound();

// Player stops moving
SoundManager.Instance.StopMovementSound();

*/

using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixers")]
    public AudioMixer audioMixer;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;

    [Header("Music")]
    public AudioSource musicSource;

    [Header("Ambient Sounds")]
    public AudioSource ambientSourceA;
    public AudioSource ambientSourceB;
    private bool isFadingToB = false;

    [Header("SFX")]
    public AudioSource sfxSource;
    public List<AudioClip> sfxClips;

    [Header("Player Loop Sounds")]
    public AudioSource playerLoopSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateMixerVolumes();
    }

    void Update()
    {
        UpdateMixerVolumes();
    }

    private void UpdateMixerVolumes()
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        audioMixer.SetFloat("AmbientVolume", Mathf.Log10(ambientVolume) * 20);
    }

    // ---- Music ----
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    // ---- SFX ----
    public void PlaySFX(AudioClip clip, Vector3 position, bool useDoppler = false)
    {
        GameObject sfxObj = new GameObject("SFX");
        sfxObj.transform.position = position;
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f; // 3D sound
        source.dopplerLevel = useDoppler ? 1f : 0f;
        source.volume = sfxVolume;
        source.Play();
        Destroy(sfxObj, clip.length);
    }

    // ---- Player Loop Sounds ----
    public void PlayPlayerLoop(bool isRunning)
    {
        playerLoopSource.clip = isRunning ? runClip : walkClip;
        if (!playerLoopSource.isPlaying)
            playerLoopSource.Play();
    }

    public void StopPlayerLoop()
    {
        playerLoopSource.Stop();
    }

    // ---- Ambient Crossfade ----
    public void CrossfadeAmbient(AudioClip newClip, float duration = 2f)
    {
        if (isFadingToB)
        {
            StartCoroutine(FadeAmbient(ambientSourceB, ambientSourceA, newClip, duration));
        }
        else
        {
            StartCoroutine(FadeAmbient(ambientSourceA, ambientSourceB, newClip, duration));
        }
        isFadingToB = !isFadingToB;
    }

    private System.Collections.IEnumerator FadeAmbient(AudioSource fromSource, AudioSource toSource, AudioClip newClip, float duration)
    {
        toSource.clip = newClip;
        toSource.volume = 0f;
        toSource.Play();

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            fromSource.volume = Mathf.Lerp(ambientVolume, 0f, t);
            toSource.volume = Mathf.Lerp(0f, ambientVolume, t);
            time += Time.deltaTime;
            yield return null;
        }

        fromSource.Stop();
        fromSource.volume = ambientVolume;
        toSource.volume = ambientVolume;
    }
}

