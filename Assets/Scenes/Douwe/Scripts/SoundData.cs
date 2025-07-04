using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class SoundData : ScriptableObject
{
    [Header("Basic Settings")]
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop;
}
