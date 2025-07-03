using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Sources ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource ambientSource;

    [Header("---------- Audio Clips ----------")]
    public AudioClip button;
    public AudioClip boost;
    public AudioClip score;
    public AudioClip cratedestruction;

    [Header("---------- Ambient ----------")]
    public AudioSource forest;
    public AudioSource city;
}

//audio manager swap


//swap ambientSource with forest and city in the inspector to change the ambient sound