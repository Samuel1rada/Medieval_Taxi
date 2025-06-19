using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MalbersAnimations.Controller;

public class pickup_copy : MonoBehaviour
{
    [Header("Pickup Settings")]
    public List<PickupDropoffPoint> pickupPoints;
    private int currentPointIndex = 0;
    private PickupDropoffPoint activePoint;
    private bool isInPickupZone = false;
    public GameObject Passenger;

    [Header("Scoring")]
    public float baseFare = 100f;
    public float fastMultiplier = 1.5f;
    public float normalMultiplier = 1.0f;
    public float slowMultiplier = 0.5f;

    [Header("Global Event Bonuses & Penalties")]
    public float globalDriveByBonus = 20f;
    public float globalDriveByPenalty = 15f;
    public float globalDestructionBonus = 25f;
    public float globalDestructionPenalty = 20f;

    public string driveByTag = "DriveByPoint";
    public string destructionTag = "Destructible";

    [Header("Cooldowns & Thresholds")]
    public float jobCooldown = 5f;
    public float dropoffSpeedThreshold = 0.2f;
    public float driveByCooldown = 0.5f;
    public float destructionCooldown = 0.5f;

    [Header("UI Elements")]
    public GameObject jobPanel;
    public TextMeshProUGUI destinationText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dropOffHint;
    public GameObject pickupIndicator;
    public Image destinationImage; // Assign in inspector
    public Image driveByPreferenceImage;      // Assign in inspector
    public Image destructionPreferenceImage;  // Assign in inspector

    [Header("Preference Sprites")]
    public Sprite driveByLikedSprite;
    public Sprite driveByDislikedSprite;
    public Sprite driveByNeutralSprite;
    public Sprite destructionLikedSprite;
    public Sprite destructionDislikedSprite;
    public Sprite destructionNeutralSprite;

    [Header("Score Manager")]
    public ScoreManager scoreManager;

    [Header("Timer Thresholds (multipliers)")]
    public float goldMultiplier = 1.3f;    // Gold: baseTime * goldMultiplier
    public float silverMultiplier = 2f;  // Silver: baseTime * silverMultiplier
    public float bronzeMultiplier = 2.7f;  // Bronze: baseTime * bronzeMultiplier

    public float baseTime = 10f; // Base time in seconds for gold (edit in inspector)
    public float averageSpeed = 10f; // Units per second, edit in inspector

    private bool isOnTrip = false;
    private bool isInDropoffZone = false;
    private float tripStartTime;
    private float estimatedTime = 10f; // Calculated per trip
    private float cooldownEndTime = 0f;
    private float lastDriveByTime = -1f;
    private float lastDestructionTime = -1f;
    private Rigidbody rb;


    public PickUpCharacterAnimation passengerAnimationController;

    // Reference to Malbers Input component (assign in inspector or auto-find)
    public MAnimal MInput; // Reference to Malbers Input component

    [Header("Emoji & Audio")]
    public Sprite likeEmojiSprite;          // Assign in inspector (sprite)
    public Sprite dislikeEmojiSprite;       // Assign in inspector (sprite)
    public AudioClip likeAudioClip;         // Assign in inspector
    public AudioClip dislikeAudioClip;      // Assign in inspector
    public Canvas uiCanvas;                 // Assign your main UI canvas here
    public float emojiAnimDuration = 0.7f;
    public Image emojiPopupImage;           // Assign in inspector: UI Image for emoji popup

    private AudioSource audioSource;

    // Emoji cooldown
    private float lastEmojiTime = -1f;
    public float emojiCooldown = 0.5f; // seconds

    [Header("Preference Sliders")]
    public Slider driveBySlider;        // Assign in inspector
    public Slider destructionSlider;    // Assign in inspector

    // Add separate cooldown trackers for bonuses
    private float lastDriveByBonusTime = -1f;
    private float lastDestructionBonusTime = -1f;
    public float driveByBonusCooldown = 2f;      // seconds, set in inspector
    public float destructionBonusCooldown = 2f;  // seconds, set in inspector

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (jobPanel != null) jobPanel.SetActive(false);
        if (dropOffHint != null) dropOffHint.text = "";
        if (pickupIndicator != null) pickupIndicator.SetActive(false);
        Passenger.SetActive(false);

        // Auto-assign MInput if not set
        if (MInput == null)
        {
            MInput = GetComponent<MAnimal>();
            if (MInput == null)
            {
                Debug.LogWarning("MAnimal (MInput) not assigned or found on this GameObject! Please assign it in the inspector.");
            }
        }

        if (passengerAnimationController != null)
        {
            passengerAnimationController.OnPickupAnimationComplete += OnPassengerAnimationComplete;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Trip initiation
        if (!isOnTrip && isInPickupZone)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartTrip(pickupPoints[currentPointIndex]);
                isInPickupZone = false;
                dropOffHint.text = "";
            }
        }

        // Trip logic
        if (isOnTrip)
        {
            Passenger.SetActive(true);

            // Only update timer if player input is enabled (MInput.enabled)
            if (MInput != null && MInput.enabled)
            {
                float elapsedTime = Time.time - tripStartTime;

                if (timerText != null)
                {
                    timerText.text = FormatTime(elapsedTime);

                    // Use estimatedTime for thresholds
                    float goldTime = estimatedTime * goldMultiplier;
                    float silverTime = estimatedTime * silverMultiplier;
                    float bronzeTime = estimatedTime * bronzeMultiplier;

                    if (elapsedTime < goldTime)
                        timerText.color = new Color32(255, 215, 0, 255); // Gold
                    else if (elapsedTime < silverTime)
                        timerText.color = new Color32(192, 192, 192, 255); // Silver
                    else if (elapsedTime < bronzeTime)
                        timerText.color = new Color32(205, 127, 50, 255); // Bronze
                    else
                        timerText.color = Color.black;
                }
            }

            // Pickup indicator image logic (support UI Image and SpriteRenderer)
            if (pickupIndicator != null && activePoint != null)
            {
                Vector3 direction = (activePoint.pointTransform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);

                // Enable UI Image if present
                var img = pickupIndicator.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = true;

                // Enable SpriteRenderer if present (for worldspace indicators)
                var sprite = pickupIndicator.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.enabled = true;
            }

            if (isInDropoffZone && rb.linearVelocity.magnitude < dropoffSpeedThreshold)
            {
                dropOffHint.text = "Press E to Drop Off";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CompleteTrip();
                }
            }
            else
            {
                dropOffHint.text = "";
            }
        }
    }

    void ShowEmoji(bool isLike)
    {
        // Emoji cooldown logic
        if (Time.time - lastEmojiTime < emojiCooldown) return;
        lastEmojiTime = Time.time;

        // Use assigned Image component for popup
        if (emojiPopupImage == null) return;
        Sprite sprite = isLike ? likeEmojiSprite : dislikeEmojiSprite;
        AudioClip clip = isLike ? likeAudioClip : dislikeAudioClip;
        if (sprite == null) return;

        emojiPopupImage.sprite = sprite;
        emojiPopupImage.enabled = true;
        emojiPopupImage.transform.localScale = Vector3.zero;

        StartCoroutine(AnimateEmojiImage(emojiPopupImage, emojiAnimDuration));

        // Play audio
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private System.Collections.IEnumerator AnimateEmojiImage(Image img, float duration)
    {
        float half = duration * 0.5f;
        float timer = 0f;
        // Grow
        while (timer < half)
        {
            float t = timer / half;
            img.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            timer += Time.deltaTime;
            yield return null;
        }
        img.transform.localScale = Vector3.one;
        // Shrink
        timer = 0f;
        while (timer < half)
        {
            float t = timer / half;
            img.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            timer += Time.deltaTime;
            yield return null;
        }
        img.transform.localScale = Vector3.zero;
        img.enabled = false;
    }

    void OnPassengerAnimationComplete()
    {
        Debug.Log("Passenger animation complete - attempting to re-enable controls");

        if (passengerAnimationController != null)
        {
            passengerAnimationController.SpawnSmokeAtPassenger();
        }

        if (MInput != null)
        {
            MInput.enabled = true;
            // Reset and start timer when input is re-enabled
            tripStartTime = Time.time;
            if (timerText != null)
                timerText.text = "0:00:000";
            Debug.Log("Controls should be re-enabled now");
        }
        else
        {
            Debug.LogError("MInput reference is null!");
        }
    }

    void OnDestroy()
    {
        if (passengerAnimationController != null)
        {
            passengerAnimationController.OnPickupAnimationComplete -= OnPassengerAnimationComplete;
        }
    }

    void StartTrip(PickupDropoffPoint pickupPoint)
    {
        MInput.enabled = false;
        // Sequentially select the next dropoff point in the list (wrap around)
        int nextIndex = (currentPointIndex + 1) % pickupPoints.Count;
        activePoint = pickupPoints[nextIndex];

        // Calculate estimated time based on distance and averageSpeed
        float distance = Vector3.Distance(pickupPoint.pointTransform.position, activePoint.pointTransform.position);
        estimatedTime = (averageSpeed > 0f) ? distance / averageSpeed : baseTime;

        isOnTrip = true;
        // tripStartTime = Time.time; // <-- REMOVE this line, timer will start after animation

        if (jobPanel != null)
            jobPanel.SetActive(true);

        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(true);
            var img = pickupIndicator.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.enabled = true;
            var sprite = pickupIndicator.GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.enabled = true;
        }

        if (destinationText != null)
            destinationText.text = $"Next Stop: {activePoint.pointName}";

        // Set the destination image sprite from the next dropoff point
        if (destinationImage != null && activePoint.pointSprite != null)
            destinationImage.sprite = activePoint.pointSprite;

        // Set the drive-by preference image and slider
        if (driveByPreferenceImage != null)
        {
            if (activePoint.driveByPreference == PreferenceLevel.Like)
                driveByPreferenceImage.sprite = driveByLikedSprite;
            else if (activePoint.driveByPreference == PreferenceLevel.Neutral)
                driveByPreferenceImage.sprite = driveByNeutralSprite;
            else
                driveByPreferenceImage.sprite = driveByDislikedSprite;
        }
        if (driveBySlider != null)
            driveBySlider.value = (int)activePoint.driveByPreference;

        // Set the destruction preference image and slider
        if (destructionPreferenceImage != null)
        {
            if (activePoint.destructionPreference == PreferenceLevel.Like)
                destructionPreferenceImage.sprite = destructionLikedSprite;
            else if (activePoint.destructionPreference == PreferenceLevel.Neutral)
                destructionPreferenceImage.sprite = destructionNeutralSprite;
            else
                destructionPreferenceImage.sprite = destructionDislikedSprite;
        }
        if (destructionSlider != null)
            destructionSlider.value = (int)activePoint.destructionPreference;

        // Reset timer display to zero at job start
        if (timerText != null)
            timerText.text = "0:00:000";

        if (pickupPoint.passengerAnimation != null)
        {
            pickupPoint.passengerAnimation.StartPickupAnimation(transform);
        }

        Passenger.SetActive(true);
        Invoke("OnPassengerAnimationComplete", 6f);

    }

    void CompleteTrip()
    {
        float tripTime = Time.time - tripStartTime;
        float multiplier = tripTime < 10f ? fastMultiplier : tripTime < 20f ? normalMultiplier : slowMultiplier;
        float score = baseFare * multiplier;
        scoreManager.AddScore(score);

        Debug.Log($"Trip completed. Score: {score}");

        isOnTrip = false;
        isInDropoffZone = false;
        // cooldownEndTime = Time.time + jobCooldown; // Remove cooldown

        if (jobPanel != null) jobPanel.SetActive(false);
        if (dropOffHint != null) dropOffHint.text = "";
        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(false);
            // Disable UI Image if present
            var img = pickupIndicator.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.enabled = false;
            // Disable SpriteRenderer if present
            var sprite = pickupIndicator.GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.enabled = false;
        }

        // ✅ Call reset logic if assigned
        if (pickupPoints[currentPointIndex].passengerAnimation != null)
        {
            pickupPoints[currentPointIndex].passengerAnimation.ResetPassenger();
        }

        Passenger.SetActive(false);

        // Move to the next pickup point in sequence (wrap around)
        currentPointIndex = (currentPointIndex + 1) % pickupPoints.Count;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isOnTrip)
        {
            // Find which pickup point this is
            for (int i = 0; i < pickupPoints.Count; i++)
            {
                if (other.transform == pickupPoints[i].pointTransform)
                {
                    isInPickupZone = true;
                    currentPointIndex = i; // Set current pickup index to this point
                    dropOffHint.text = "Press E to Start Trip";
                    break;
                }
            }
        }

        if (isOnTrip)
        {
            if (other.transform == activePoint.pointTransform)
            {
                isInDropoffZone = true;
            }

            if (other.CompareTag(driveByTag) && Time.time - lastDriveByTime >= driveByCooldown)
            {
                lastDriveByTime = Time.time;

                // Bonus/penalty cooldown logic
                if (Time.time - lastDriveByBonusTime >= driveByBonusCooldown)
                {
                    float scoreChange = ScoreManager.GetScoreForPreference(activePoint.driveByPreference, globalDriveByBonus, globalDriveByPenalty);
                    scoreManager.AddScore(scoreChange);

                    if (activePoint.driveByPreference == PreferenceLevel.Like)
                        ShowEmoji(true);
                    else if (activePoint.driveByPreference == PreferenceLevel.Dislike)
                        ShowEmoji(false);

                    Debug.Log($"DriveBy event. Score change: {scoreChange}");
                    lastDriveByBonusTime = Time.time;
                }
                else
                {
                    Debug.Log("DriveBy bonus/penalty on cooldown.");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isOnTrip && other.transform == pickupPoints[currentPointIndex].pointTransform)
        {
            isInPickupZone = false;
            dropOffHint.text = "";
        }

        if (isOnTrip && other.transform == activePoint.pointTransform)
        {
            isInDropoffZone = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isOnTrip || !collision.gameObject.CompareTag(destructionTag)) return;
        if (Time.time - lastDestructionTime < destructionCooldown) return;

        lastDestructionTime = Time.time;

        // Bonus/penalty cooldown logic
        if (Time.time - lastDestructionBonusTime >= destructionBonusCooldown)
        {
            float scoreChange = ScoreManager.GetScoreForPreference(activePoint.destructionPreference, globalDestructionBonus, globalDestructionPenalty);
            scoreManager.AddScore(scoreChange);

            if (activePoint.destructionPreference == PreferenceLevel.Like)
                ShowEmoji(true);
            else if (activePoint.destructionPreference == PreferenceLevel.Dislike)
                ShowEmoji(false);

            Debug.Log($"Destruction event. Score change: {scoreChange}");
            lastDestructionBonusTime = Time.time;
        }
        else
        {
            Debug.Log("Destruction bonus/penalty on cooldown.");
        }
    }

    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        int millis = Mathf.FloorToInt((seconds - mins * 60 - secs) * 1000);
        return $"{mins}:{secs:00}:{millis:000}";
    }
}
