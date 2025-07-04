using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MalbersAnimations.Controller;

/// <summary>
/// Handles the pickup and dropoff system for passengers, including trip logic, scoring, UI, and event feedback.
/// This script is the "brain" of the medieval taxi game. If you break it, the peasants will riot.
/// </summary>
public class SimplifiedPickUpSystem : MonoBehaviour
{
    // === Inspector Fields ===

    [Header("Pickup Settings")]
    public List<PickupDropoffPoint> pickupPoints; // List of pick up points
    private int currentPointIndex = 0;            // Track which pickup point we're at
    private PickupDropoffPoint activePoint;       // Where we dropping?
    private bool isInPickupZone = false;          // Are we ready to pick up a passenger?
    public GameObject Passenger;                  // The actual passenger object (don't lose them!)

    [Header("Scoring")]
    public float baseFare = 100f;                 // How much a trip is worth (before tips)
    public float fastMultiplier = 1.5f;           // Im fast pay me more
    public float normalMultiplier = 1.0f;         // Idc how much you pay me, I just want to drive
    public float slowMultiplier = 0.5f;           // Wow! Look at the scenery!

    [Header("Global Event Bonuses & Penalties")]
    public float globalDriveByBonus = 20f;        // hey Im walking here bonus
    public float globalDriveByPenalty = 15f;      // I have walked here get out
    public float globalDestructionBonus = 25f;    // CART SMASH!
    public float globalDestructionPenalty = 20f;  // Oops, too much smash

    public string driveByTag = "NPC";    // Tag for drive-by triggers (like a medieval checkpoint)
    public string destructionTag = "Obstacle";// Tag for destructible objects (barrels, crates, dreams)

    [Header("Cooldowns & Thresholds")]
    public float jobCooldown = 5f;                // Time before you can get another job (unused, but feels official)
    public float dropoffSpeedThreshold = 0.2f;    // How slow you must go to drop off (no drifting into dropoffs!)
    public float driveByCooldown = 0.5f;          // Can't spam drive-bys
    public float destructionCooldown = 0.5f;      // Can't spam destruction either

    [Header("UI Elements")]
    public GameObject jobPanel;                   // The big UI panel that tells you what to do
    public TextMeshProUGUI destinationText;       // Where are we going?
    public TextMeshProUGUI timerText;             // How long have we been driving?
    public TextMeshProUGUI dropOffHint;           // Helpful hints for the player
    public GameObject pickupIndicator;            // The magical arrow that points the way
    public Image destinationImage;                // A picture is worth a thousand words
    public Image driveByPreferenceImage;          // Shows if the passenger likes drive-bys
    public Image destructionPreferenceImage;      // Shows if the passenger likes chaos

    [Header("Preference Sprites")]
    public Sprite driveByLikedSprite;             // "Yay, drive-bys!"
    public Sprite driveByDislikedSprite;          // "Boo, drive-bys!"
    public Sprite driveByNeutralSprite;           // "Meh, drive-bys."
    public Sprite destructionLikedSprite;         // "Yay, destruction!"
    public Sprite destructionDislikedSprite;      // "Boo, destruction!"
    public Sprite destructionNeutralSprite;       // "Meh, destruction."

    [Header("Score Manager")]
    public ScoreManager scoreManager;             // The keeper of all points

    [Header("Timer Thresholds (multipliers)")]
    public float goldMultiplier = 1.3f;           // Be fast for gold!
    public float silverMultiplier = 2f;           // Silver is still shiny
    public float bronzeMultiplier = 2.7f;         // Bronze is for finishers

    public float baseTime = 10f;                  // The "par" time for a trip
    public float averageSpeed = 10f;              // How fast we expect you to go

    [Header("Debug & Visualization")]
    // public bool showSpeedFeedbackAlways = false; // Show speed feedback even if not in pickup zone
    public bool visualizePickupZone = false;     // Visualize pickup point proximity

    // === State Variables ===

    private bool isOnTrip = false;                // Are we currently on a trip?
    private bool isInDropoffZone = false;         // Are we ready to drop off?
    private float tripStartTime;                  // When did this trip start?
    private float estimatedTime = 10f;            // How long should this trip take?
    private float cooldownEndTime = 0f;           // (Unused) When can we take another job?
    private float lastDriveByTime = -1f;          // When was the last drive-by?
    private float lastDestructionTime = -1f;      // When was the last destruction?
    private Rigidbody rb;                         // For checking our speed (no cheating!)

    public PickUpCharacterAnimation passengerAnimationController; // The animation overlord

    public MAnimal MInput;                        // Controls for the medieval taxi (horse, cart, etc.)

    [Header("Emoji & Audio")]
    public Sprite likeEmojiSprite;                // "Nice job!" emoji
    public Sprite dislikeEmojiSprite;             // "Boo!" emoji
    public AudioClip likeAudioClip;               // Happy sound
    public AudioClip dislikeAudioClip;            // Sad sound
    public Canvas uiCanvas;                       // The canvas of destiny
    public float emojiAnimDuration = 0.7f;        // How long the emoji dances
    public Image emojiPopupImage;                 // Where the emoji appears

    private AudioSource audioSource;              // For making noise

    private float lastEmojiTime = -1f;            // When did we last show an emoji?
    public float emojiCooldown = 0.5f;            // Don't spam emojis

    [Header("Preference Sliders")]
    public Slider driveBySlider;                  // Shows drive-by preference numerically
    public Slider destructionSlider;              // Shows destruction preference numerically

    private float lastDriveByBonusTime = -1f;     // When did we last give a drive-by bonus?
    private float lastDestructionBonusTime = -1f; // When did we last give a destruction bonus?
    public float driveByBonusCooldown = 2f;       // Can't spam drive-by bonuses
    public float destructionBonusCooldown = 2f;   // Can't spam destruction bonuses

    [Header("Controller stuff")]
    private bool usingController = false;             // Are we using a game controller?
    private float lastInputDeviceCheckTime = 0f;      // When did we last check input device?
    public float inputDeviceCheckInterval = 2f;       // How often to check for input device changes

    // === Unity Methods ===

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (jobPanel != null) jobPanel.SetActive(false); // Hide job panel at start
        if (dropOffHint != null) dropOffHint.text = "";  // Clear dropoff hint
        if (pickupIndicator != null) pickupIndicator.SetActive(false); // Hide pickup indicator
        Passenger.SetActive(false); // Hide passenger at start

        // If you forgot to assign MInput, we'll try to find it for you!
        if (MInput == null)
        {
            MInput = GetComponent<MAnimal>();
            if (MInput == null)
            {
                Debug.LogWarning("MAnimal (MInput) not assigned or found on this GameObject! Please assign it in the inspector.");
            }
        }

        // Listen for when the animation is done so we can give control back to the player
        if (passengerAnimationController != null)
        {
            passengerAnimationController.OnPickupAnimationComplete += OnPassengerAnimationComplete;
        }

        // Make sure we can play sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(PickupZoneChecker());
        // Remove DetectInputDevice() from Start, always check in Update instead
    }

    void Update()
    {
        // Always check input device every frame
        DetectInputDevice();

        // Visual feedback for proximity to pickup point (even if not in pickup zone)
        if (visualizePickupZone && !isOnTrip)
        {
            bool nearAny = false;
            for (int i = 0; i < pickupPoints.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, pickupPoints[i].pointTransform.position);
                if (dist < 6f && dist >= 3f) // Near but not close enough
                {
                    if (dropOffHint != null)
                        dropOffHint.text = "<color=yellow>Get closer to pick up!</color>";
                    nearAny = true;
                    break;
                }
            }
            if (!nearAny && dropOffHint != null && !isInPickupZone)
                dropOffHint.text = "";
        }

        // Waiting for the player to start a trip? Listen for E or controller button!
        if (!isOnTrip && isInPickupZone)
        {
            // Check speed before allowing pickup
            if (rb != null && rb.linearVelocity.magnitude > dropoffSpeedThreshold)
            {
                if (dropOffHint != null)
                    dropOffHint.text = "<color=red>Slow down to pick up!</color>";
            }
            else
            {
                if (dropOffHint != null)
                {
                    dropOffHint.text = usingController
                        ? "Press Y/Triangle to Start Trip"
                        : "Press E to Start Trip";
                }
                if ((usingController && Input.GetKeyDown(KeyCode.JoystickButton3)) ||
                    (!usingController && Input.GetKeyDown(KeyCode.E)))
                {
                    StartTrip(pickupPoints[currentPointIndex]);
                    isInPickupZone = false;
                    dropOffHint.text = "";
                }
            }
        }

        // If we're on a trip, update everything!
        if (isOnTrip)
        {
            // Update the timer and make it shiny if you're fast!
            if (MInput != null && MInput.enabled)
            {
                float elapsedTime = Time.time - tripStartTime;

                if (timerText != null)
                {
                    timerText.text = FormatTime(elapsedTime);

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

            // Spin the magical pickup indicator to point the way!
            if (pickupIndicator != null && activePoint != null)
            {
                Vector3 direction = (activePoint.pointTransform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);

                var img = pickupIndicator.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.enabled = true;

                var sprite = pickupIndicator.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.enabled = true;
            }

            // Only allow dropoff if you're basically stopped (no drive-thru dropoffs!)
            if (isInDropoffZone)
            {
                if (rb.linearVelocity.magnitude < dropoffSpeedThreshold)
                {
                    if (dropOffHint != null)
                    {
                        dropOffHint.text = usingController
                            ? "Press Y/Triangle to Drop Off"
                            : "Press E to Drop Off";
                    }
                    if ((usingController && Input.GetKeyDown(KeyCode.JoystickButton3)) ||
                        (!usingController && Input.GetKeyDown(KeyCode.E)))
                    {
                        CompleteTrip();
                    }
                }
                else
                {
                    dropOffHint.text = "<color=red>Slow down to drop off!</color>";
                }
            }
            else
            {
                dropOffHint.text = "";
            }
        }
    }

    // === Emoji Feedback ===

    /// <summary>
    /// Shows an emoji popup and plays audio feedback for like/dislike events.
    /// If you make the passenger happy or mad, let them show it!
    /// </summary>
    void ShowEmoji(bool isLike)
    {
        if (Time.time - lastEmojiTime < emojiCooldown) return;
        lastEmojiTime = Time.time;

        if (emojiPopupImage == null) return;
        Sprite sprite = isLike ? likeEmojiSprite : dislikeEmojiSprite;
        AudioClip clip = isLike ? likeAudioClip : dislikeAudioClip;
        if (sprite == null) return;

        emojiPopupImage.sprite = sprite;
        emojiPopupImage.enabled = true;
        emojiPopupImage.transform.localScale = Vector3.zero;

        StartCoroutine(AnimateEmojiImage(emojiPopupImage, emojiAnimDuration));

        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Coroutine to animate the emoji popup image.
    /// The emoji grows, then shrinks, like your ego after a code review.
    /// </summary>
    private System.Collections.IEnumerator AnimateEmojiImage(Image img, float duration)
    {
        float half = duration * 0.5f;
        float timer = 0f;
        while (timer < half)
        {
            float t = timer / half;
            img.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            timer += Time.deltaTime;
            yield return null;
        }
        img.transform.localScale = Vector3.one;
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

    // === Animation Event Callback ===

    /// <summary>
    /// Called when the passenger pickup animation is complete.
    /// Re-enables controls and resets timer.
    /// This is where the magic happens after the cutscene!
    /// </summary>
    void OnPassengerAnimationComplete()
    {
        Debug.Log("Passenger animation complete - attempting to re-enable controls");

        if (passengerAnimationController != null)
        {
            passengerAnimationController.SpawnSmokeAtPassenger();
            // === Camera returns to cameraReturnTarget when input is enabled ===
            passengerAnimationController.CameraLookAtReturnTarget();
        }

        if (MInput != null)
        {
            MInput.enabled = true;
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
        // Unsubscribe from animation event to avoid memory leaks
        if (passengerAnimationController != null)
        {
            passengerAnimationController.OnPickupAnimationComplete -= OnPassengerAnimationComplete;
        }
    }

    // === Trip Logic ===

    /// <summary>
    /// Starts a new trip from the given pickup point.
    /// Sets up UI, destination, and triggers the pickup animation.
    /// This is the start of every adventure!
    /// </summary>
    void StartTrip(PickupDropoffPoint pickupPoint)
    {
        MInput.enabled = false;
        int nextIndex = (currentPointIndex + 1) % pickupPoints.Count;
        activePoint = pickupPoints[nextIndex];

        float distance = Vector3.Distance(pickupPoint.pointTransform.position, activePoint.pointTransform.position);
        estimatedTime = (averageSpeed > 0f) ? distance / averageSpeed : baseTime;

        isOnTrip = true;

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

        if (destinationImage != null && activePoint.pointSprite != null)
            destinationImage.sprite = activePoint.pointSprite;

        // Set drive-by preference UI
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

        // Set destruction preference UI
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

        if (timerText != null)
            timerText.text = "0:00:000";

        // Start the pickup animation for the passenger
        if (pickupPoint.passengerAnimation != null)
        {
            pickupPoint.passengerAnimation.StartPickupAnimation(transform);
        }

        // Fallback: ensure controls are re-enabled if animation event fails
        Invoke("OnPassengerAnimationComplete", 6f);
    }

    // ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣤⠴⠶⠖⠒⠒⠒⠶⠤⣤⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡞⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⠓⠶⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⠏⠀⠀⠀⠀⠀⠀⠀⠰⡆⠀⠀⠀⠀⣠⣀⣀⠀⠀⠀⠙⠳⣤⠀⠀⠀⠀⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⠀⠀⣰⠋⠀⠀⠀⠀⠀⠀⠰⠂⠀⠰⢤⡙⢆⠀⠈⠿⠋⠀⠀⠀⠀⠀⠈⢳⡀⠀⠀⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⠀⢰⠏⠀⠀⠀⠀⠀⠀⢀⣠⣤⣶⣶⡠⡝⢎⣧⠀⠀⣀⠤⠔⠀⠠⠀⠀⠀⢻⡄⠀⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⢠⡟⠀⠀⠀⠀⠀⠀⠀⢻⡟⠁⢀⣨⣽⣿⣦⠹⣤⠊⠀⢀⠀⠀⠀⠀⠐⠂⠀⢻⡄⠀⠀⠀
    // ⠀⠀⠀⠀⠀⠀⡾⠀⠀⠀⠀⠀⣀⠀⠀⠸⡇⠀⡿⢿⡿⡟⣿⣿⠻⢤⣶⣿⡦⣶⣤⡀⠀⠣⠀⠈⣷⠀⠀⠀
    // ⠀⠀⠀⠀⠀⢸⠇⠀⠀⠀⠀⠈⠉⠀⠀⠀⠙⣄⣈⢛⣚⣅⠉⡉⠀⠰⣏⠛⠓⠛⠻⠇⠀⠀⠀⠀⢻⠀⠀⠀
    // ⢠⣴⢿⡉⠓⣾⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠿⠛⢡⣴⣇⣀⠀⠈⣹⣆⠀⠀⠀⠈⠃⠀⠀⢸⠀⠀⠀
    // ⣿⡷⡀⢧⠀⣿⠀⠀⠀⠀⠀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠟⠛⣋⣁⠚⠛⢿⣿⣇⠀⠀⠀⠀⠀⠀⡿⠀⠀⠀
    // ⢻⡇⠙⠈⠂⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⡶⠋⡠⣬⣿⣭⣿⣽⣿⠀⠀⠀⠀⠀⠀⡿⠀⠀⠀
    // ⠘⡇⠀⠀⠀⠘⢧⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣾⠿⣤⣾⣷⣾⣷⣶⣾⡿⠃⣸⠀⠀⠀⠀⣸⠇⠀⠀⠀
    // ⠀⠹⣄⠀⠀⠀⠀⠙⢦⣄⠀⠀⠀⠀⠀⠀⠀⡰⠋⡃⢸⣿⣿⣿⣿⢿⣿⣤⠀⢰⡏⠀⠀⠀⣠⡟⠀⠀⠀⠀
    // ⠀⠀⠈⢧⡀⠀⠀⠀⠀⠙⠳⣦⣀⠀⠀⠀⠀⠳⣀⠻⠄⣉⣛⠻⢿⢿⠟⢛⣽⠀⠀⠀⠀⢰⡟⣀⣀⡴⢶⡄
    // ⠀⠀⠀⠀⠑⢄⠀⠀⢀⣠⠞⠉⠙⠳⣦⡀⠀⠀⠘⠦⠄⠀⠈⠉⠽⣿⣿⠟⠀⠀⠀⢀⣴⠟⠙⣻⣿⠖⢦⣿
    // ⠀⠀⠀⠀⠀⢀⡿⢯⠉⠀⠀⠀⠀⠀⠀⠙⠳⠦⠤⣤⣄⣀⣀⣀⣀⣀⣀⣤⡤⠶⠞⠋⠁⣠⠞⠁⠈⣳⠘⠃
    // ⠀⠀⠀⢀⡴⠋⠀⠈⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⢉⡽⠋⠀⠀⠀⠀⢀⡤⠚⠁⠀⠀⠀⠋⠀⠀
    // ⠀⢀⡴⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡾⢀⣀⣀⣀⣠⠖⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀
    // ⠀⡾⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡠⠋⠀⠀⠀⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    // ⠀⠓⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠼⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
    // 
    // 
    //  JUST WORK DAMMIT!!!


    /// <summary>
    /// Completes the current trip, calculates score, resets UI, and handles passenger logic.
    /// This is the finish line! Tally up the points, reset the world, and get ready for the next ride.
    /// </summary>
    void CompleteTrip()
    {
        float tripTime = Time.time - tripStartTime;
        float multiplier = tripTime < 10f ? fastMultiplier : tripTime < 20f ? normalMultiplier : slowMultiplier;
        float score = baseFare * multiplier;
        scoreManager.AddScore(score);

        Debug.Log($"Trip completed. Score: {score}");

        isOnTrip = false;
        isInDropoffZone = false;

        if (jobPanel != null) jobPanel.SetActive(false);
        if (dropOffHint != null) dropOffHint.text = "";
        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(false);
            var img = pickupIndicator.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.enabled = false;
            var sprite = pickupIndicator.GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.enabled = false;
        }

        // Reset and hide the passenger, and spawn smoke at cart after dropoff
        if (pickupPoints[currentPointIndex].passengerAnimation != null)
        {
            pickupPoints[currentPointIndex].passengerAnimation.ResetPassenger();
            pickupPoints[currentPointIndex].passengerAnimation.SetCartPassengerInactive();
            pickupPoints[currentPointIndex].passengerAnimation.SpawnSmokeAtCartPassenger();
        }

        // Move to the next pickup point in the list
        currentPointIndex = (currentPointIndex + 1) % pickupPoints.Count;
    }

    // === Trigger and Collision Logic ===

    /// <summary>
    /// Handles entering pickup/dropoff zones and drive-by events.
    /// If you see this, you're either picking up, dropping off, or causing chaos.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!isOnTrip)
        {
            // Check if entering a pickup zone by player tag
            for (int i = 0; i < pickupPoints.Count; i++)
            {
                if (other.CompareTag("Player"))
                {
                    // Check if the player is within the pickup point's trigger collider
                    float dist = Vector3.Distance(other.transform.position, pickupPoints[i].pointTransform.position);
                    if (dist < 3f) // Adjust radius as needed
                    {
                        isInPickupZone = true;
                        currentPointIndex = i;
                        break;
                    }
                }
            }
        }

        if (isOnTrip)
        {
            // Check if entering dropoff zone
            if (activePoint != null && other.transform == activePoint.pointTransform)
            {
                isInDropoffZone = true;
            }

            // Handle drive-by event
            if (other.CompareTag(driveByTag) && Time.time - lastDriveByTime >= driveByCooldown)
            {
                lastDriveByTime = Time.time;

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
        // Only clear pickup zone if we are not on a trip anymore
        if (!isOnTrip)
        {
            for (int i = 0; i < pickupPoints.Count; i++)
            {
                if (other.CompareTag("Player"))
                {
                    float dist = Vector3.Distance(other.transform.position, pickupPoints[i].pointTransform.position);
                    if (dist < 3f) // Adjust radius as needed
                    {
                        isInPickupZone = false;
                        dropOffHint.text = "";
                        break;
                    }
                }
            }
        }

        if (isOnTrip && activePoint != null && other.transform == activePoint.pointTransform)
        {
            isInDropoffZone = false;
        }
    }

    /// <summary>
    /// Handles collision with destructible objects for scoring and feedback.
    /// If you hit something breakable, let's see if the passenger likes it!
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (!isOnTrip || !collision.gameObject.CompareTag(destructionTag)) return;
        if (Time.time - lastDestructionTime < destructionCooldown) return;

        lastDestructionTime = Time.time;

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


    // === Timer Formatting ===
    /// <summary>
    /// Formats a float time value into a string mm:ss:ms.
    /// Because nobody likes ugly timers.
    /// </summary>
    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        int millis = Mathf.FloorToInt((seconds - mins * 60 - secs) * 1000);
        return $"{mins}:{secs:00}:{millis:000}";
    }

    // Checks every second if player is inside a pickup zone and no job is active
    private System.Collections.IEnumerator PickupZoneChecker()
    {
        while (true)
        {
            if (!isOnTrip)
            {
                bool found = false;
                for (int i = 0; i < pickupPoints.Count; i++)
                {
                    // Check for any collider with tag "Player" within a radius of the pickup point
                    Collider[] colliders = Physics.OverlapSphere(pickupPoints[i].pointTransform.position, 3f); // Adjust radius as needed
                    foreach (var col in colliders)
                    {
                        if (col.CompareTag("Player"))
                        {
                            isInPickupZone = true;
                            currentPointIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (!found)
                {
                    isInPickupZone = false;
                    if (dropOffHint != null)
                        dropOffHint.text = "";
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Detects whether the player is using a controller or keyboard/mouse.
    /// Because peasants can't decide how they want to control their carts.
    /// </summary>
    void DetectInputDevice()
    {
        // Check for controller input
        string[] joystickNames = Input.GetJoystickNames();
        bool controllerConnected = false;

        foreach (string name in joystickNames)
        {
            if (!string.IsNullOrEmpty(name))
            {
                controllerConnected = true;
                break;
            }
        }
        bool controllerInput =
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ||
            Input.GetKey(KeyCode.JoystickButton0) ||
            Input.GetKey(KeyCode.JoystickButton1) ||
            Input.GetKey(KeyCode.JoystickButton2) ||
            Input.GetKey(KeyCode.JoystickButton3);

        // Only switch to controller mode if we actually get controller input
        if (controllerConnected && controllerInput)
        {
            usingController = true;
        }
        // Switch back to keyboard if we get keyboard input
        else if (Input.anyKeyDown && !controllerInput)
        {
            usingController = false;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (visualizePickupZone && pickupPoints != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            foreach (var point in pickupPoints)
            {
                if (point != null && point.pointTransform != null)
                {
                    Gizmos.DrawSphere(point.pointTransform.position, 3f); // Pickup radius
                    Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
                    Gizmos.DrawSphere(point.pointTransform.position, 6f); // "Near" radius
                    Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                }
            }
        }
    }
}
#endif

//  "Good work Agent 47, All bugs have been eliminated, Please proceed to the next mission."
// 
//                        .-""""-.
//                       / j      \
//                      :.d;       ;
//                      $$P        :
//           .m._       $$         :
//          dSMMSSSss.__$$b.    __ :
//         :MMSMMSSSMMMSS$$$b  $$P ;
//         SMMMSMMSMMMSSS$$$$     :b
//        dSMMMSMMMMMMSSMM$$$b.dP SSb.
//       dSMMMMMMMMMMSSMMPT$$=-. /TSSSS.
//      :SMMMSMMMMMMMSMMP  `$b_.'  MMMMSS.
//      SMMMMMSMMMMMMMMM \  .'\    :SMMMSSS.
//     dSMSSMMMSMMMMMMMM  \/\_/; .'SSMMMMSSSm
//    dSMMMMSMMSMMMMMMMM    :.;'" :SSMMMMSSMM;
//  .MMSSSSSMSSMMMMMMMM;    :.;   MMSMMMMSMMM;
// dMSSMMSSSSSSSMMMMMMM;    ;.;   MMMMMMMSMMM
//:MMMSSSSMMMSSP^TMMMMM     ;.;   MMMMMMMMMMM
//MMMSMMMMSSSSP   `MMMM     ;.;   :MMMMMMMMM;
//"TMMMMMMMMMM      TM;    :`.:    MMMMMMMMM;
//   )MMMMMMM;     _/\\    :`.:    :MMMMMMMM
//  d$SS$$$MMMb.  |._\\\   :`.:     MMMMMMMM
//  T$$S$$$$$$$$$$m;O\\\\"-;`.:_.-  MMMMMMM;
// :$$$$$$$$$$$$$$$b_l./\\ ;`.:    mMMSSMMM;
// :$$$$$$$$$$$$$$$$$$$./\\;`.:  .$$MSMMMMMM
//  $$$$$$$$$$$$$$$$$$$$. \\`.:.$$$$SMSSSMMM;
//  $$$$$$$$$$$$$$$$$$$$$. \\.:$$$$$SSMMMMMMM
//  :$$$$$$$$$$$$$$$$$$$$$.//.:$$$$SSSSSSSMM;
//  :$$$$$$$$$$$$$$$$$$$$$$.`.:$$SSSSSSSMMMP
//   $$$$$$$$$;"^$J "^$$$$;.`.$$P  `SSSMMMM
//   :$$$$$$$$$       :$$$;.`.P'..   TMMM$$b
//   :$$$$$$$$$;      $$$$;.`/ c^'   d$$$$$S;
//   $$$$$S$$$$;      '^^^:_d$g:___.$$$$$$SSS
//   $$$$SS$$$$;            $$$$$$$$$$$$$$SSS;
//  :$$$SSSS$$$$            : $$$$$$$$$$$$$SSS
//  :$P"TSSSS$$$            ; $$$$$$$$$$$$$SSS;
//  j    `SSSSS$           :  :$$$$$$$$$$$$$SS$
// :       "^S^'           :   $$$$$$$$$$$$$S$;
// ;.____.-;"               "--^$$$$$$$$$$$$$P
// '-....-"                       ""^^T$$$$P"