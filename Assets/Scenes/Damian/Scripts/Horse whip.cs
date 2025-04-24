using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HorseWhipSound : MonoBehaviour
{
    [Header("Whip Sound Settings")]
    public AudioClip whipSound;
    public float cooldownDuration = 10f; 
    public float movementThreshold = 0.2f; 
    public KeyCode whipKey = KeyCode.LeftShift; 

    private AudioSource audioSource;
    private float cooldownTimer;
    private bool isOnCooldown = false;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
            }
            return; 
        }

        bool isMoving = (transform.position - lastPosition).magnitude > movementThreshold;
        lastPosition = transform.position;

        bool whipKeyPressed = Input.GetKey(whipKey);

        if ((isMoving || whipKeyPressed) && whipSound != null)
        {
            PlayWhipSound();
        }
    }

    void PlayWhipSound()
    {
        audioSource.PlayOneShot(whipSound);
        StartCooldown();
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
    }

    public void TriggerWhip()
    {
        if (!isOnCooldown && whipSound != null)
        {
            PlayWhipSound();
        }
    }
}