using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName; // Name of the point (editable in the Inspector)
    public Transform pointTransform; // Transform of the point
}

public class PickUpSystem : MonoBehaviour
{
    public List<PickupDropoffPoint> pickupDropoffPoints; // List of pick-up/drop-off points with names
    public GameObject pickupIndicator; // Game object to show above the player when a pickup is active
    public float baseFare = 10f; // Base fare for a trip
    public float speedMultiplierFast = 1.5f; // Multiplier for faster trips
    public float speedMultiplierNormal = 1f; // Multiplier for on-time trips
    public float speedMultiplierSlow = 0.5f; // Multiplier for slower trips
    public float comfortMultiplier = 1f; // Multiplier for comfort (can be adjusted by another script)
    public float looksMultiplier = 1f; // Multiplier for looks (can be adjusted by another script)
    public float cooldownTime = 5f; // Cooldown time before a new job can start
    public float timeMultiplier = 2f; // Multiplier to increase estimated time

    private Transform currentPickupPoint;
    private Transform currentDropoffPoint;
    private bool isPickupActive = false;
    private bool isOnCooldown = false;
    private float estimatedTime;
    private float estimatedDistance;
    private float tripStartTime;
    private float cooldownEndTime;

    void Start()
    {
        if (pickupIndicator == null)
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }
        else
        {
            pickupIndicator.SetActive(false); // Hide the pickup indicator at the start
        }

        if (pickupDropoffPoints == null || pickupDropoffPoints.Count == 0)
        {
            Debug.LogError("No pick-up/drop-off points assigned!");
        }
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

                // Adjust the rotation to ensure the arrow points horizontally (X rotation = 90 degrees)
                pickupIndicator.transform.rotation = Quaternion.Euler(90f, targetRotation.eulerAngles.y, 0f);
            }

            // Check if the player has reached the drop-off point
            if (Vector3.Distance(transform.position, currentDropoffPoint.position) < 2f)
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

        if (!isPickupActive && !isOnCooldown)
        {
            // Check if the player is at a pick-up point
            foreach (var point in pickupDropoffPoints)
            {
                if (point.pointTransform == other.transform)
                {
                    Debug.Log("Pick-up point detected: " + point.pointName);
                    StartTrip(point.pointTransform);
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Collided with object, but a trip is already active or on cooldown.");
        }
    }

    void StartTrip(Transform pickupPoint)
    {
        currentPickupPoint = pickupPoint;
        currentDropoffPoint = GetRandomDropoffPoint(pickupPoint);

        if (currentDropoffPoint == null)
        {
            Debug.LogError("Failed to find a valid drop-off point!");
            return;
        }

        estimatedDistance = Vector3.Distance(currentPickupPoint.position, currentDropoffPoint.position);
        estimatedTime = (estimatedDistance / 10f) * timeMultiplier; // Increase estimated time with multiplier
        tripStartTime = Time.time;
        isPickupActive = true;

        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(true); // Show the pickup indicator
        }
        else
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }

        Debug.Log("Trip started! Pick-up: " + GetPointName(currentPickupPoint) + ", Drop-off: " + GetPointName(currentDropoffPoint));
        Debug.Log("Estimated Distance: " + estimatedDistance + ", Estimated Time: " + estimatedTime);
    }

    void DropOffPassenger()
    {
        float tripTime = Time.time - tripStartTime;
        float payment = CalculatePayment(tripTime);
        Debug.Log("Passenger dropped off! Trip Time: " + tripTime + ", Payment: " + payment);
        isPickupActive = false;

        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(false); // Hide the pickup indicator
        }
        else
        {
            Debug.LogError("Pickup Indicator is not assigned!");
        }

        // Start cooldown
        StartCooldown();
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

        if (timeDifference < -10f) // Faster than estimated time
        {
            speedMultiplier = speedMultiplierFast;
            Debug.Log("Trip completed faster than estimated time!");
        }
        else if (timeDifference >= -10f && timeDifference <= 10f) // Within +-10 seconds of estimated time
        {
            speedMultiplier = speedMultiplierNormal;
            Debug.Log("Trip completed within estimated time!");
        }
        else // Slower than estimated time
        {
            speedMultiplier = speedMultiplierSlow;
            Debug.Log("Trip completed slower than estimated time!");
        }

        // Calculate final payment with multipliers
        float payment = baseFare * speedMultiplier * comfortMultiplier * looksMultiplier;
        return payment;
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
        } while (dropoffPoint == pickupPoint); // Ensure the drop-off point is not the same as the pick-up point

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

    // Public methods to adjust multipliers
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