using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite destinationImage;
}

public class PickUpSystem : MonoBehaviour
{
    public List<PickupDropoffPoint> pickupDropoffPoints;
    public GameObject pickupIndicator;
    public float baseFare = 10f;
    public float speedMultiplierFast = 1.5f;
    public float speedMultiplierNormal = 1f;
    public float speedMultiplierSlow = 0.5f;
    public float comfortMultiplier = 1f;
    public float looksMultiplier = 1f;
    public float cooldownTime = 5f;
    public float timeMultiplier = 1.3f;
    public float maxSpeedForJobActivation = 5f;
    public float maxSpeedForDropoff = 0.1f;

    public test popupSystem;
    public GameObject jobUIPanel;
    public Image destinationImageUI;
    public TextMeshProUGUI destinationText; 
    public TextMeshProUGUI timerText; 
    public Animator jobUIAnimator;

    [Header("Bean Management")]
    public List<GameObject> beanList; // List of regular beans
    public GameObject passengerBean; // The passenger bean

    private Transform currentPickupPoint;
    private Transform currentDropoffPoint;
    private bool isPickupActive = false;
    private bool isOnCooldown = false;
    private float estimatedTime;
    private float estimatedDistance;
    private float tripStartTime;
    private float cooldownEndTime;
    private Rigidbody playerRigidbody;

    void Start()
    {
        if (pickupIndicator == null)
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }
        else
        {
            pickupIndicator.SetActive(false);
        }

        if (pickupDropoffPoints == null || pickupDropoffPoints.Count == 0)
        {
            Debug.LogError("No pick-up/drop-off points assigned!");
        }
        if (popupSystem == null)
        {
            Debug.LogError("Popup System is not assigned!");
        }

        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Player Rigidbody is not found!");
        }

        if (jobUIPanel != null)
        {
            jobUIPanel.SetActive(false);
        }


        // Initialize bean states
        SetBeanStates(false);
    }

    void SetBeanStates(bool jobActive)
    {
        // Enable/disable beans based on job status
        foreach (var bean in beanList)
        {
            if (bean != null)
            {
                bean.SetActive(!jobActive);
            }
        }

        // Passenger bean is active only when job is active
        if (passengerBean != null)
        {
            passengerBean.SetActive(jobActive);
        }
    }

    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds - Mathf.Floor(timeInSeconds)) * 1000);

        return string.Format("{0}:{1:00},{2:000}", minutes, seconds, milliseconds);
    }

    void Update()
    {
        if (isPickupActive)
        {
            // Point the arrow toward the drop-off point
            if (pickupIndicator != null && currentDropoffPoint != null)
            {
                Vector3 direction = (currentDropoffPoint.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);
            }

            // Update the timer UI
            if (timerText != null)
            {
                float elapsedTime = Time.time - tripStartTime;
                timerText.text = FormatTime(elapsedTime);

                // Change the timer text color based on trip time
                if (elapsedTime < estimatedTime)
                {
                    timerText.color = Color.yellow; // Gold for faster trips
                }
                else if (elapsedTime >= estimatedTime && elapsedTime <= estimatedTime + 10f)
                {
                    timerText.color = Color.gray; // Silver for on-time trips
                }
                else
                {
                    timerText.color = new Color(0.8f, 0.5f, 0.2f); // Bronze for slower trips
                }
            }

            // Check if the player has reached the drop-off point and is standing still
            if (Vector3.Distance(transform.position, currentDropoffPoint.position) < 2f && playerRigidbody.linearVelocity.magnitude < maxSpeedForDropoff)
            {
                DropOffPassenger();
            }
        }

        // Handle cooldown
        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            isOnCooldown = false;
            Debug.Log("Cooldown ended. Ready for a new job!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called with: " + other.name);

        if (!isPickupActive && !isOnCooldown && playerRigidbody.linearVelocity.magnitude < maxSpeedForJobActivation)
        {
            foreach (var point in pickupDropoffPoints)
            {
                if (point.pointTransform == other.transform)
                {
                    Debug.Log("Pick-up point detected: " + point.pointName);
                    StartTrip(point);
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Collided with object, but a trip is already active, on cooldown, or player is moving too fast.");
        }
    }

    void StartTrip(PickupDropoffPoint pickupPoint)
    {
        currentPickupPoint = pickupPoint.pointTransform;
        currentDropoffPoint = GetRandomDropoffPoint(pickupPoint.pointTransform);

        if (currentDropoffPoint == null)
        {
            Debug.LogError("Failed to find a valid drop-off point!");
            return;
        }

        estimatedDistance = Vector3.Distance(currentPickupPoint.position, currentDropoffPoint.position);
        estimatedTime = (estimatedDistance / 10f) * timeMultiplier;
        tripStartTime = Time.time;
        isPickupActive = true;

        // Update bean states
        SetBeanStates(true);

        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(true);
        }
        else
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }

        // Show the job UI panel with animation
        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIPanel.SetActive(true);
            jobUIAnimator.ResetTrigger("SlideOut"); // Reset the SlideOut trigger
            jobUIAnimator.SetTrigger("SlideIn"); // Trigger the SlideIn animation

            // Set the destination image and text
            if (destinationImageUI != null)
            {
                destinationImageUI.sprite = pickupPoint.destinationImage;
            }
            if (destinationText != null)
            {
                destinationText.text = "Destination: " + GetPointName(currentDropoffPoint);
            }
        }

        Debug.Log("Trip started! Pick-up: " + pickupPoint.pointName + ", Drop-off: " + GetPointName(currentDropoffPoint));
        Debug.Log("Estimated Distance: " + estimatedDistance + ", Estimated Time: " + estimatedTime);
    }

    void DropOffPassenger()
    {
        float tripTime = Time.time - tripStartTime;
        float payment = CalculatePayment(tripTime);
        Debug.Log("Passenger dropped off! Trip Time: " + tripTime + ", Payment: " + payment);

        // Trigger the popup with the calculated payment
        if (popupSystem != null)
        {
            popupSystem.owned_cash += (int)payment;
            popupSystem.mypopup.textMeshPro.text = "Cash gained: " + payment.ToString("F2");
            popupSystem.amount.text = "Money: " + popupSystem.owned_cash.ToString();
            popupSystem.mypopup.animator.SetTrigger("fadein");
            popupSystem.timerIsRunning = true;
            popupSystem.onscreen = true;
        }
        else
        {
            Debug.LogError("Popup System is not assigned!");
        }

        isPickupActive = false;

        // Update bean states
        SetBeanStates(false);

        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(false);
        }
        else
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }

        // Hide the job UI panel with animation
        if (jobUIPanel != null && jobUIAnimator != null)
        {
            jobUIAnimator.SetTrigger("SlideOut"); // Trigger the SlideOut animation
            StartCoroutine(DeactivateJobUIPanelAfterAnimation());
        }

        // Start cooldown
        StartCooldown();
    }

    System.Collections.IEnumerator DeactivateJobUIPanelAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f); // Wait for the SlideOut animation to finish
        jobUIPanel.SetActive(false);
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownTime;
        Debug.Log("Cooldown started. Next job available in " + cooldownTime + " seconds.");
    }

    float CalculatePayment(float tripTime)
    {
        float timeDifference = tripTime - estimatedTime;
        float speedMultiplier;

        if (timeDifference < -10f)
        {
            speedMultiplier = speedMultiplierFast;
            Debug.Log("Trip completed faster than estimated time!");
        }
        else if (timeDifference >= -10f && timeDifference <= 10f)
        {
            speedMultiplier = speedMultiplierNormal;
            Debug.Log("Trip completed within estimated time!");
        }
        else
        {
            speedMultiplier = speedMultiplierSlow;
            Debug.Log("Trip completed slower than estimated time!");
        }

        return baseFare * speedMultiplier * comfortMultiplier * looksMultiplier;
    }

    Transform GetRandomDropoffPoint(Transform pickupPoint)
    {
        if (pickupDropoffPoints.Count < 2)
        {
            Debug.LogError("Not enough pick-up/drop-off points assigned!");
            return null;
        }

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
            {
                return point.pointName;
            }
        }
        return "Unknown Point";
    }

    public void SetComfortMultiplier(float multiplier)
    {
        comfortMultiplier = multiplier;
        Debug.Log("Comfort Multiplier set to: " + comfortMultiplier);
    }

    public void SetLooksMultiplier(float multiplier)
    {
        looksMultiplier = multiplier;
        Debug.Log("Looks Multiplier set to: " + looksMultiplier);
    }
}