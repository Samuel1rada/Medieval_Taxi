using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using MalbersAnimations;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite destinationImage;
    public NPCPref passengerPreferences;
}

public class PickUpSystem : MonoBehaviour
{
    private MInput inputComponent;

    [SerializeField] private Transform playerTransform;
    public List<PickupDropoffPoint> pickupDropoffPoints;
    private PickupDropoffPoint currentPickupPointData;
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

    [Header("Job Animation")]
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private MInput malbersInput; // Drag in the Inspector

    [Header("Animation Cooldown")]
    public float cameraAnimationCooldown = 10f; // Time in seconds between animations
    private float lastCameraAnimationTime = -10f; // Initialize to allow first animation


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
        malbersInput = FindObjectOfType<MInput>(); // Make sure only one exists or find it on the player

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
                DropOffText.text = "Press E or (Y/Triangle) to drop off passenger";
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
        // Store the ENTIRE pickup point data, not just the transform
        currentPickupPointData = pickupPoint;
        currentPickupPoint = pickupPoint.pointTransform;
        currentDropoffPoint = GetRandomDropoffPoint(pickupPoint.pointTransform);

        if (currentDropoffPoint == null) return;

        // Set NPC personality and preferences
        NPCPref passengerPrefs = pickupPoint.pointTransform.GetComponent<NPCPref>();
        if (passengerPrefs == null)
            passengerPrefs = pickupPoint.pointTransform.gameObject.AddComponent<NPCPref>();

        passengerPrefs.preset = (NPCPref.PersonalityPreset)Random.Range(1, System.Enum.GetValues(typeof(NPCPref.PersonalityPreset)).Length);
        passengerPrefs.ApplySelectedPreset();
        pickupPoint.passengerPreferences = passengerPrefs;

        // Disable player input
        malbersInput.SetInput("All", false);

        // Check if camera animation is off cooldown
        bool playAnimation = Time.time - lastCameraAnimationTime >= cameraAnimationCooldown;

        if (playAnimation && cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartCameraIntro");
            lastCameraAnimationTime = Time.time;
        }

        // Start the job immediately if no animation, or wait for animation if playing
        if (playAnimation)
        {
            StartCoroutine(WaitForCameraIntroThenStartJob(pickupPoint));
        }
        else
        {
            FinalizeJobStart(pickupPoint);
        }
    }

    void DropOffPassenger()
    {
        float tripTime = Time.time - tripStartTime;
        NPCPref passenger = GetCurrentPassengerPreferences();

        if (passenger != null && scoreManager != null)
        {
            PreferenceSettings journeyStats = new PreferenceSettings
            {
                npcFast = tripTime < estimatedTime - 10f,
                npcDriveBy = false,
                npcDestruction = false,
                npcRamps = false
            };

            ScoreManager.DeliverySpeed deliverySpeed = GetDeliverySpeed(tripTime);

            scoreManager.ShowFinalScore(
                passenger.GetPreferences(),
                journeyStats,
                deliverySpeed
            );
        }

        ResetDropOffState();
        StartCooldown();
    }

    private void ResetDropOffState()
    {
        if (DropOffText != null)
        {
            DropOffText.text = "";
            DropOffText.alpha = 0f;
        }

        isPickupActive = false;
        isInDropoffTrigger = false;
        SetBeanStates(false);

        if (pickupIndicator != null)
            pickupIndicator.SetActive(false);

        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIAnimator.SetTrigger("SlideOut");
            StartCoroutine(DeactivateJobUIPanelAfterAnimation());
        }
    }

    private ScoreManager.DeliverySpeed GetDeliverySpeed(float tripTime)
    {
        float timeDifference = tripTime - estimatedTime;

        if (timeDifference < -10f) return ScoreManager.DeliverySpeed.Fast;
        if (timeDifference <= 10f) return ScoreManager.DeliverySpeed.Medium;
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
        if (timeDifference < -10f) return speedMultiplierFast;
        if (timeDifference <= 10f) return speedMultiplierNormal;
        return speedMultiplierSlow;
    }

    Transform GetRandomDropoffPoint(Transform pickupPoint)
    {
        if (pickupDropoffPoints.Count < 2) return null;

        // Find the pickup point in the list to get its data
        PickupDropoffPoint pickupData = pickupDropoffPoints.Find(p => p.pointTransform == pickupPoint);
        if (pickupData == null) return null;

        // Get a random dropoff point (excluding the pickup point)
        List<PickupDropoffPoint> possibleDropoffs = pickupDropoffPoints.FindAll(p => p.pointTransform != pickupPoint);
        if (possibleDropoffs.Count == 0) return null;

        int randomIndex = Random.Range(0, possibleDropoffs.Count);
        return possibleDropoffs[randomIndex].pointTransform;
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
            scoreManager.HandleRampUsed(playerTransform, passenger.GetPreferences());
            Debug.Log($"Ramp used! Height: {rampHeight}");
        }
    }

    IEnumerator WaitForCameraIntroThenStartJob(PickupDropoffPoint pickupPoint)
    {
        // Wait for animation duration (you might want to make this exact)
        yield return new WaitForSeconds(2f);
        FinalizeJobStart(pickupPoint);
    }

    void FinalizeJobStart(PickupDropoffPoint pickupPoint)
    {
        estimatedDistance = Vector3.Distance(currentPickupPoint.position, currentDropoffPoint.position);
        estimatedTime = (estimatedDistance / 10f) * timeMultiplier;
        tripStartTime = Time.time;
        isPickupActive = true;

        SetBeanStates(true);
        if (pickupIndicator != null) pickupIndicator.SetActive(true);
        malbersInput.SetInput("All", true);

        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIPanel.SetActive(true);
            jobUIAnimator.ResetTrigger("SlideOut");
            jobUIAnimator.SetTrigger("SlideIn");

            // Use the stored pickup point's destination image
            if (destinationImageUI != null)
                destinationImageUI.sprite = currentPickupPointData.destinationImage;

            if (destinationText != null)
                destinationText.text = "Destination: " + GetPointName(currentDropoffPoint);
        }

        Debug.Log($"New passenger personality: {pickupPoint.passengerPreferences.preset}");
    }
}