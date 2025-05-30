using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MalbersAnimations.InputSystem;

public class SimplifiedPickUpSystem : MonoBehaviour
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
    [Header("Score Manager")]
    public ScoreManager scoreManager;

    private bool isOnTrip = false;
    private bool isInDropoffZone = false;
    private float tripStartTime;
    private float cooldownEndTime = 0f;
    private float lastDriveByTime = -1f;
    private float lastDestructionTime = -1f;
    private Rigidbody rb;


    public PickUpCharacterAnimation passengerAnimationController;

    // Reference to Malbers Input component (assign in inspector or auto-find)
    public Component malbersInputComponent; // Assign your Malbers Input component in the inspector
    private bool inputLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (jobPanel != null) jobPanel.SetActive(false);
        if (dropOffHint != null) dropOffHint.text = "";
        if (pickupIndicator != null) pickupIndicator.SetActive(false);
        Passenger.SetActive(false);

        // Auto-detect Malbers input component if not set
        // Ensure it's assigned in the Inspector.
        if (malbersInputComponent == null)
        {
            // Try to find a component with InputEnabled property
            malbersInputComponent = GetComponent("MInput") ?? GetComponent("InputController") ?? GetComponent("MalbersInput");
            if (malbersInputComponent == null)
            {
                Debug.LogWarning("Malbers Input Component not assigned or found on this GameObject! Please assign it in the inspector.");
            }
        }


        if (passengerAnimationController != null)
        {
            passengerAnimationController.OnPickupAnimationComplete += OnPassengerAnimationComplete;
        }
    }

    void Update()
    {
        // Automatically lock/unlock input based on animation state
        if (passengerAnimationController != null && passengerAnimationController.IsAnimating)
        {
            LockPlayerInput();
        }
        else if (inputLocked)
        {
            UnlockPlayerInput();
        }

        // Trip initiation
        if (!isOnTrip && isInPickupZone && Time.time >= cooldownEndTime)
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
            float elapsedTime = Time.time - tripStartTime;

            if (timerText != null)
                timerText.text = $"{FormatTime(elapsedTime)}";

            if (pickupIndicator != null && activePoint != null)
            {
                Vector3 direction = (activePoint.pointTransform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);
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

    void LockPlayerInput()
    {
        if (!inputLocked && malbersInputComponent != null)
        {
            var prop = malbersInputComponent.GetType().GetProperty("InputEnabled");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(malbersInputComponent, false, null);
            }
            inputLocked = true;
        }
    }

    void UnlockPlayerInput()
    {
        if (inputLocked && malbersInputComponent != null)
        {
            var prop = malbersInputComponent.GetType().GetProperty("InputEnabled");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(malbersInputComponent, true, null);
            }
            inputLocked = false;
        }
    }

    void OnPassengerAnimationComplete()
    {
        UnlockPlayerInput();
        if (passengerAnimationController != null)
        {
            passengerAnimationController.SpawnSmokeAtPassenger();
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
        // Randomly select a dropoff point that isn't the pickup point
        List<PickupDropoffPoint> possibleDropoffs = new List<PickupDropoffPoint>(pickupPoints);
        possibleDropoffs.Remove(pickupPoint);
        activePoint = possibleDropoffs[Random.Range(0, possibleDropoffs.Count)];

        isOnTrip = true;
        tripStartTime = Time.time;

        if (jobPanel != null)
            jobPanel.SetActive(true);

        if (pickupIndicator != null)
            pickupIndicator.SetActive(true);

        if (destinationText != null)
            destinationText.text = $"Next Stop: {activePoint.pointName}";

        if (pickupPoint.passengerAnimation != null)
        {
            pickupPoint.passengerAnimation.StartPickupAnimation(transform);
            // Lock input immediately
            LockPlayerInput();
        }

        Passenger.SetActive(true);
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
        cooldownEndTime = Time.time + jobCooldown;

        if (jobPanel != null) jobPanel.SetActive(false);
        if (dropOffHint != null) dropOffHint.text = "";
        if (pickupIndicator != null) pickupIndicator.SetActive(false);

        // ✅ Call reset logic if assigned
        if (pickupPoints[currentPointIndex].passengerAnimation != null)
        {
            pickupPoints[currentPointIndex].passengerAnimation.ResetPassenger();
        }

        Passenger.SetActive(false);
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
                float scoreChange = Random.value > 0.5f ? globalDriveByBonus : -globalDriveByPenalty;
                scoreManager.AddScore(scoreChange);
                Debug.Log($"DriveBy event. Score change: {scoreChange}");
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
        float scoreChange = Random.value > 0.5f ? globalDestructionBonus : -globalDestructionPenalty;
        scoreManager.AddScore(scoreChange);
        Debug.Log($"Destruction event. Score change: {scoreChange}");
    }

    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{mins:D2}:{secs:D2}";
    }
}
