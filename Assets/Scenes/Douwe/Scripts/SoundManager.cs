using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource movementSource;

    [Header("Music Settings")]
    [Tooltip("Background music clip")]
    [SerializeField] private SoundData backgroundMusic;

    [Header("Ambient Settings")]
    [Tooltip("Current ambient sound")]
    [SerializeField] private SoundData ambientSound;

    [Header("Player Movement Sounds")]
    [Tooltip("Sound played during player movement")]
    [SerializeField] private SoundData movementSound;

    [Header("General SFX")]
    [Tooltip("Array of general sound effects")]
    [SerializeField] private SoundData[] sfxClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayBackgroundMusic();
        PlayAmbientSound();
    }

    #region Music
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        musicSource.clip = backgroundMusic.clip;
        musicSource.volume = backgroundMusic.volume;
        musicSource.pitch = backgroundMusic.pitch;
        musicSource.loop = backgroundMusic.loop;
        musicSource.Play();
    }
    #endregion

    #region Ambient
    public void PlayAmbientSound()
    {
        if (ambientSound == null || ambientSource == null) return;
        ambientSource.clip = ambientSound.clip;
        ambientSource.volume = ambientSound.volume;
        ambientSource.pitch = ambientSound.pitch;
        ambientSource.loop = ambientSound.loop;
        ambientSource.Play();
    }

    public void TransitionAmbient(SoundData newAmbient, float transitionTime = 1f)
    {
        StartCoroutine(FadeAmbient(newAmbient, transitionTime));
    }

    private System.Collections.IEnumerator FadeAmbient(SoundData newAmbient, float time)
    {
        float startVolume = ambientSource.volume;
        float timer = 0f;

        while (timer < time)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, 0, timer / time);
            timer += Time.deltaTime;
            yield return null;
        }

        ambientSource.clip = newAmbient.clip;
        ambientSource.volume = 0;
        ambientSource.Play();

        timer = 0f;
        while (timer < time)
        {
            ambientSource.volume = Mathf.Lerp(0, newAmbient.volume, timer / time);
            timer += Time.deltaTime;
            yield return null;
        }
        ambientSource.volume = newAmbient.volume;
    }
    #endregion

    #region Movement
    public void PlayMovementSound()
    {
        if (movementSound == null || movementSource == null || movementSource.isPlaying) return;
        movementSource.clip = movementSound.clip;
        movementSource.volume = movementSound.volume;
        movementSource.pitch = movementSound.pitch;
        movementSource.loop = movementSound.loop;
        movementSource.Play();
    }

    public void StopMovementSound()
    {
        if (movementSource.isPlaying)
            movementSource.Stop();
    }
    #endregion

    #region SFX
    public void PlaySFX(string name)
    {
        SoundData sfx = System.Array.Find(sfxClips, s => s.soundName == name);
        if (sfx != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(sfx.clip, sfx.volume);
        }
        else
        {
            Debug.LogWarning("SFX not found: " + name);
        }
    }
    #endregion
}
