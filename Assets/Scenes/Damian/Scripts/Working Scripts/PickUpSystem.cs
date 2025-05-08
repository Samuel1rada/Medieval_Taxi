using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite destinationImage;
    public NPCPref passengerPreferences; // This should hold a reference to the NPC's preferences
}

public class PickUpSystem : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    public List<PickupDropoffPoint> pickupDropoffPoints;
    public GameObject pickupIndicator;
    public float baseFare = 10f;
    public float speedMultiplierFast = 1.5f;
    public float speedMultiplierNormal = 1f;
    public float speedMultiplierSlow = 0.5f;
    public float cooldownTime = 5f;
    public float timeMultiplier = 1.2f;
    public float maxSpeedForJobActivation = 5f;
    public float maxSpeedForDropoff = 0.1f;
    private bool isInDropoffTrigger = false;

    [Header("UI Elements")]
    public TextMeshProUGUI DropOffText;
    public float dropOffFade = 2f;
    public GameObject jobUIPanel;
    public Image destinationImageUI;
    public TextMeshProUGUI destinationText;
    public TextMeshProUGUI timerText;
    public Animator jobUIAnimator;

    [Header("Bean Management")]
    public List<GameObject> beanList;
    public GameObject passengerBean;

    private Transform currentPickupPoint;
    private Transform currentDropoffPoint;
    private bool isPickupActive = false;
    private bool isOnCooldown = false;
    private float estimatedTime;
    private float estimatedDistance;
    private float tripStartTime;
    private float cooldownEndTime;
    private Rigidbody playerRigidbody;

    [Header("Scoring System")]
    public ScoreManager scoreManager;

    void Start()
    {
        if (pickupIndicator != null)
            pickupIndicator.SetActive(false);

        if (DropOffText != null)
        {
            DropOffText.text = "";
            DropOffText.alpha = 0f;
        }

        playerRigidbody = GetComponent<Rigidbody>();
        if (jobUIPanel != null)
            jobUIPanel.SetActive(false);

        if (Gamepad.current == null)
            Debug.Log("No gamepad detected - keyboard controls only");
        else
            Debug.Log("Gamepad detected: " + Gamepad.current.name);

        SetBeanStates(false);
    }

    void SetBeanStates(bool jobActive)
    {
        foreach (var bean in beanList)
            if (bean != null) bean.SetActive(!jobActive);

        if (passengerBean != null)
            passengerBean.SetActive(jobActive);
    }

    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int deciseconds = Mathf.FloorToInt((timeInSeconds - Mathf.Floor(timeInSeconds)) * 10);
        return $"{minutes}:{seconds:00},{deciseconds:0}";
    }

    void Update()
    {
        if (isPickupActive)
        {
            if (pickupIndicator != null && currentDropoffPoint != null)
            {
                Vector3 direction = (currentDropoffPoint.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);
            }

            if (timerText != null)
            {
                float elapsedTime = Time.time - tripStartTime;
                timerText.text = FormatTime(elapsedTime);
                timerText.color = elapsedTime < estimatedTime ? Color.yellow :
                                  elapsedTime <= estimatedTime + 10f ? Color.gray :
                                  new Color(0.8f, 0.5f, 0.2f);
            }

            UpdateDropOffText();
        }

        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            isOnCooldown = false;
            Debug.Log("Cooldown ended. Ready for a new job!");
        }

    }

    void UpdateDropOffText()
    {
        if (DropOffText == null || !isPickupActive) return;

        bool isSlowEnough = playerRigidbody.linearVelocity.magnitude < maxSpeedForDropoff;
        bool controllerNorthPressed = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;

        if (isInDropoffTrigger)
        {
            if (isSlowEnough)
            {
                DropOffText.text = "Press E or (Y/△) to drop off passenger";
                DropOffText.color = Color.green;

                if (Input.GetKeyDown(KeyCode.E) || controllerNorthPressed)
                    DropOffPassenger();
            }
            else
            {
                DropOffText.text = "Too fast to drop off passenger";
                DropOffText.color = Color.red;
            }

            DropOffText.alpha = Mathf.Lerp(DropOffText.alpha, 1f, dropOffFade * Time.deltaTime);
        }
        else
        {
            DropOffText.alpha = Mathf.Lerp(DropOffText.alpha, 0f, dropOffFade * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isPickupActive && !isOnCooldown &&
            playerRigidbody.linearVelocity.magnitude < maxSpeedForJobActivation)
        {
            foreach (var point in pickupDropoffPoints)
            {
                if (point.pointTransform == other.transform)
                {
                    StartTrip(point);
                    break;
                }
            }
        }
        else if (isPickupActive && other.transform == currentDropoffPoint)
        {
            isInDropoffTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isPickupActive && other.transform == currentDropoffPoint)
            isInDropoffTrigger = false;
    }

    void StartTrip(PickupDropoffPoint pickupPoint)
    {
        currentPickupPoint = pickupPoint.pointTransform;
        currentDropoffPoint = GetRandomDropoffPoint(pickupPoint.pointTransform);
        if (currentDropoffPoint == null) return;

        // Get or add NPCPref component to the pickup point if it doesn't exist
        NPCPref passengerPrefs = pickupPoint.pointTransform.GetComponent<NPCPref>();
        if (passengerPrefs == null)
        {
            passengerPrefs = pickupPoint.pointTransform.gameObject.AddComponent<NPCPref>();
        }

        // Randomly select a personality preset (excluding Custom)
        passengerPrefs.preset = (NPCPref.PersonalityPreset)Random.Range(1, System.Enum.GetValues(typeof(NPCPref.PersonalityPreset)).Length);

        // Apply the selected preset
        passengerPrefs.ApplySelectedPreset();

        estimatedDistance = Vector3.Distance(currentPickupPoint.position, currentDropoffPoint.position);
        estimatedTime = (estimatedDistance / 10f) * timeMultiplier;
        tripStartTime = Time.time;
        isPickupActive = true;
        SetBeanStates(true);
        if (pickupIndicator != null) pickupIndicator.SetActive(true);

        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIPanel.SetActive(true);
            jobUIAnimator.ResetTrigger("SlideOut");
            jobUIAnimator.SetTrigger("SlideIn");
            if (destinationImageUI != null) destinationImageUI.sprite = pickupPoint.destinationImage;
            if (destinationText != null) destinationText.text = "Destination: " + GetPointName(currentDropoffPoint);

            // Add debug info about the passenger's personality
            Debug.Log($"New passenger personality: {passengerPrefs.preset}");
        }
    }

    void DropOffPassenger()
    {
        float tripTime = Time.time - tripStartTime;
        NPCPref passenger = GetCurrentPassengerPreferences();

        if (passenger != null && scoreManager != null)
        {
            // Create simplified journey stats (adjust based on your actual gameplay factors)
            PreferenceSettings journeyStats = new PreferenceSettings
            {
                npcFast = tripTime < estimatedTime - 10f,  // Fast delivery
                npcDriveBy = false,                         // Not used in scoring
                npcDestruction = false,                     // Not used in scoring
                npcRamps = false                            // Ramp bonuses handled separately
            };

            // Calculate delivery speed
            ScoreManager.DeliverySpeed deliverySpeed = GetDeliverySpeed(tripTime);

            // Call the scoring method
            scoreManager.ShowFinalScore(
                passenger.GetPreferences(),
                journeyStats,
                deliverySpeed
            );
        }

        // Reset UI and state
        ResetDropOffState();
        StartCooldown();
    }
    private void ResetDropOffState()
    {
        // Reset UI elements
        if (DropOffText != null)
        {
            DropOffText.text = "";
            DropOffText.alpha = 0f;
        }

        // Reset pickup state
        isPickupActive = false;
        isInDropoffTrigger = false;
        SetBeanStates(false);

        if (pickupIndicator != null)
            pickupIndicator.SetActive(false);

        // Slide out job UI panel
        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIAnimator.SetTrigger("SlideOut");
            StartCoroutine(DeactivateJobUIPanelAfterAnimation());
        }
    }
    private ScoreManager.DeliverySpeed GetDeliverySpeed(float tripTime)
    {
        float timeDifference = tripTime - estimatedTime;

        if (timeDifference < -10f)
        {
            Debug.Log("Fast delivery bonus!");
            return ScoreManager.DeliverySpeed.Fast;
        }
        if (timeDifference <= 10f)
        {
            Debug.Log("On-time delivery");
            return ScoreManager.DeliverySpeed.Medium;
        }

        Debug.Log("Late delivery penalty");
        return ScoreManager.DeliverySpeed.Slow;
    }

    IEnumerator DeactivateJobUIPanelAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        jobUIPanel.SetActive(false);
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownTime;
    }

    float CalculatePayment(float tripTime)
    {
        float timeDifference = tripTime - estimatedTime;
        float speedMultiplier = timeDifference < -10f ? speedMultiplierFast :
                                timeDifference <= 10f ? speedMultiplierNormal :
                                speedMultiplierSlow;
        return baseFare * speedMultiplier;
    }
    float CalculateTimeMultiplier(float tripTime)
    {
        float timeDifference = tripTime - estimatedTime;

        if (timeDifference < -10f)
        {
            Debug.Log("Fast delivery bonus applied!");
            return speedMultiplierFast;
        }
        else if (timeDifference <= 10f)
        {
            Debug.Log("On-time delivery.");
            return speedMultiplierNormal;
        }
        else
        {
            Debug.Log("Slow delivery penalty applied.");
            return speedMultiplierSlow;
        }
    }


    Transform GetRandomDropoffPoint(Transform pickupPoint)
    {
        if (pickupDropoffPoints.Count < 2) return null;

        Transform dropoffPoint;
        do
        {
            int randomIndex = Random.Range(0, pickupDropoffPoints.Count);
            dropoffPoint = pickupDropoffPoints[randomIndex].pointTransform;
        } while (dropoffPoint == pickupPoint);

        return dropoffPoint;
    }

    string GetPointName(Transform pointTransform)
    {
        foreach (var point in pickupDropoffPoints)
        {
            if (point.pointTransform == pointTransform)
                return point.pointName;
        }
        return "Unknown Point";
    }

    private NPCPref GetCurrentPassengerPreferences()
    {
        foreach (var point in pickupDropoffPoints)
        {
            if (point.pointTransform == currentPickupPoint)
            {
                // Ensure preferences are properly initialized
                if (point.passengerPreferences == null)
                {
                    point.passengerPreferences = point.pointTransform.gameObject.AddComponent<NPCPref>();
                    point.passengerPreferences.preset = (NPCPref.PersonalityPreset)Random.Range(1, System.Enum.GetValues(typeof(NPCPref.PersonalityPreset)).Length);
                    point.passengerPreferences.ApplySelectedPreset();
                }
                return point.passengerPreferences;
            }
        }
        return null;
    }

    public void PlayerUsedRamp(int rampHeight)
    {
        if (!isPickupActive || scoreManager == null) return;

        NPCPref passenger = GetCurrentPassengerPreferences();
        if (passenger != null)
        {
            // New version (passing player transform)
            scoreManager.HandleRampUsed(playerTransform, passenger.GetPreferences());
            Debug.Log($"Ramp used! Height: {rampHeight}");
        }
    }

}
