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
    public AudioClip forest;
    public AudioClip city;
    public AudioClip score;
    public AudioClip cratedestruction;
    public AudioClip witch;


}
