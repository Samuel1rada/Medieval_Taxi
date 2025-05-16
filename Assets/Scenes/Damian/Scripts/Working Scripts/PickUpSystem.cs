using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using MalbersAnimations;
using MalbersAnimations.InputSystem;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite destinationImage;
    public bool likesDriveBy;
    public bool likesDestruction;
    [Tooltip("Score penalty when passenger dislikes this action")]
    public float driveByBonus = 25f;  // Renamed for clarity
    public float driveByPenalty = 20f;
    public float destructionBonus = 30f;  // Renamed for clarity
    public float destructionPenalty = 25f;
}

public class PickUpSystem : MonoBehaviour
{
    private MInput inputComponent;

    [SerializeField] private Transform playerTransform;
    public List<PickupDropoffPoint> pickupDropoffPoints;
    private PickupDropoffPoint currentPickupPointData;
    public GameObject pickupIndicator;
    public float baseFare = 100f;
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
    [SerializeField] private MInputLink malbersInput; // Drag in the Inspector

    [Header("Animation Cooldown")]
    public float cameraAnimationCooldown = 10f; // Time in seconds between animations
    private float lastCameraAnimationTime = -10f; // Initialize to allow first animation
    [Header("Preference Scoring")]
    public string driveByTag = "DriveByPoint";
    public string destructionTag = "Destructible";

    private bool currentLikesDriveBy;
    private bool currentLikesDestruction;
    private float lastDestructionScoreTime = -1f;
    public float globalDestructionScoreCooldown = 0.5f;




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
        Debug.Log("Entered trigger: " + other.name);

        // Original pickup/dropoff logic
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

        // Handle drive-by events with penalties
        if (isPickupActive && other.CompareTag(driveByTag))
        {
            if (currentLikesDriveBy)
            {
                scoreManager.AddScore(currentPickupPointData.driveByBonus);
                Debug.Log($"Drive-by bonus: +{currentPickupPointData.driveByBonus}");
            }
            else
            {
                scoreManager.AddScore(-currentPickupPointData.driveByPenalty);
                Debug.Log($"Drive-by penalty: -{currentPickupPointData.driveByPenalty}");
            }
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

        currentPickupPointData = pickupPoint;
        malbersInput.SetInput("All", false);

        if (Time.time - lastCameraAnimationTime >= cameraAnimationCooldown && cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartCameraIntro");
            lastCameraAnimationTime = Time.time;
            StartCoroutine(WaitForCameraIntroThenStartJob(pickupPoint));
        }
        else
        {
            FinalizeJobStart(pickupPoint);
        }
    }

    IEnumerator WaitForCameraIntroThenStartJob(PickupDropoffPoint pickupPoint)
    {
        yield return new WaitForSeconds(2f);
        FinalizeJobStart(pickupPoint);
    }

    void FinalizeJobStart(PickupDropoffPoint pickupPoint)
    {
        estimatedDistance = Vector3.Distance(currentPickupPoint.position, currentDropoffPoint.position);
        estimatedTime = (estimatedDistance / 10f) * timeMultiplier;
        tripStartTime = Time.time;
        isPickupActive = true;

        // Set current passenger preferences
        currentLikesDriveBy = pickupPoint.likesDriveBy;
        currentLikesDestruction = pickupPoint.likesDestruction;

        SetBeanStates(true);
        if (pickupIndicator != null) pickupIndicator.SetActive(true);
        malbersInput.SetInput("All", true);

        SetupJobUI();

        // Update destination text with preferences
        if (destinationText != null)
        {
            string prefText = "";
            if (currentLikesDriveBy) prefText += "Likes Drive Bys\n";
            if (currentLikesDestruction) prefText += "Likes Destruction";
            destinationText.text = $"Destination: {GetPointName(currentDropoffPoint)}\n{prefText}";
        }
    }
    void DropOffPassenger()
    {
        if (!CanDropOff()) return;

        float tripTime = Time.time - tripStartTime;
        CalculateAndApplyScore(tripTime);
        ResetDropOffState();
        StartCooldown();
    }

    private bool CanDropOff()
    {
        bool isSlowEnough = playerRigidbody.linearVelocity.magnitude < maxSpeedForDropoff;
        bool controllerNorthPressed = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        return isInDropoffTrigger && isSlowEnough && (Input.GetKeyDown(KeyCode.E) || controllerNorthPressed);
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

    Transform GetRandomDropoffPoint(Transform pickupPoint)
    {
        if (pickupDropoffPoints.Count < 2) return null;

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

    private void SetupJobUI()
    {
        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIPanel.SetActive(true);
            jobUIAnimator.ResetTrigger("SlideOut");
            jobUIAnimator.SetTrigger("SlideIn");

            if (destinationImageUI != null)
                destinationImageUI.sprite = currentPickupPointData.destinationImage;

            if (destinationText != null)
                destinationText.text = "Destination: " + GetPointName(currentDropoffPoint);
        }
    }

    void CalculateAndApplyScore(float tripTime)
    {
        if (scoreManager == null) return;

        ScoreManager.DeliverySpeed deliverySpeed = GetDeliverySpeed(tripTime);
        float speedMultiplier = GetSpeedMultiplier(deliverySpeed);
        float finalScore = baseFare * speedMultiplier;

        scoreManager.AddScore(finalScore);
    }

    private float GetSpeedMultiplier(ScoreManager.DeliverySpeed speed)
    {
        switch (speed)
        {
            case ScoreManager.DeliverySpeed.Fast: return speedMultiplierFast;
            case ScoreManager.DeliverySpeed.Medium: return speedMultiplierNormal;
            case ScoreManager.DeliverySpeed.Slow: return speedMultiplierSlow;
            default: return 1.0f;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!isPickupActive) return;
        if (!collision.gameObject.CompareTag(destructionTag)) return;

        // Global cooldown: applies to *any* destruction hit
        if (Time.time - lastDestructionScoreTime < globalDestructionScoreCooldown)
            return;

        lastDestructionScoreTime = Time.time;

        if (currentLikesDestruction)
        {
            scoreManager.AddScore(currentPickupPointData.destructionBonus);
            Debug.Log($"Destruction bonus: +{currentPickupPointData.destructionBonus}");
        }
        else
        {
            scoreManager.AddScore(-currentPickupPointData.destructionPenalty);
            Debug.Log($"Destruction penalty: -{currentPickupPointData.destructionPenalty}");
        }
    }


}