using UnityEngine;

public class horsewhipcopy : MonoBehaviour
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
    public bool whipsoundplayed = false;

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
        bool justStartedMoving = isMoving && !wasMovingLastFrame;
        wasMovingLastFrame = isMoving;
        lastPosition = transform.position;

        bool whipKeyPressed = Input.GetKeyDown(whipKey); // Changed to GetKeyDown
        bool whipkeyLetgo = Input.GetKeyUp(whipKey);

        if ((justStartedMoving || whipkeyLetgo) && whipSound != null)
        {
            if (whipsoundplayed == true)
            {
                PlayWhipSound();
            }

        }

        if (whipKeyPressed)
        {
            whipsoundplayed = true;
        }
    }

    private bool wasMovingLastFrame = false;

    void PlayWhipSound()
    {
        audioSource.PlayOneShot(whipSound);
        //StartCooldown();
        whipsoundplayed = false;
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
