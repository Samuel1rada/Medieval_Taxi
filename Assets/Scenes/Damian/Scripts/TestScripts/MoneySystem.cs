using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // For distance calculation

public class MoneySystem : MonoBehaviour
{
    [System.Serializable]
    public class TaxiPoint
    {
        public string Name;
        public Collider TriggerZone;
    }
    public GameObject waypointMarker; // Assign in Inspector
    public Transform playerCar; // Assign in Inspector


    public List<TaxiPoint> taxiPoints = new List<TaxiPoint>();
    public float baseFare = 10f;
    public float speedBonusMultiplier = 1.5f;
    public float slowPenaltyMultiplier = 0.5f;
    public float estimatedSpeed = 10f; // Unity units per second

    public float comfortMultiplier = 1.0f;
    public float looksMultiplier = 1.0f;

    private TaxiPoint pickupPoint;
    private TaxiPoint dropoffPoint;
    private float estimatedTime;
    private float startTime;
    private bool onRide = false;
    private bool playerInDropoffZone = false; // Track if player is inside the drop-off zone
    public Transform navigationArrow; // Assign this in Inspector



    void Start()
    {
        if (taxiPoints.Count < 2)
        {
            Debug.LogError("Not enough taxi points assigned!");
        }
        AssignRandomPickup();
    }

    void Update()
    {
        if (onRide && dropoffPoint != null && navigationArrow != null)
        {
            // Get direction to drop-off
            Vector3 direction = (dropoffPoint.TriggerZone.transform.position - playerCar.position).normalized;

            // Only rotate horizontally (Y-axis)
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            navigationArrow.rotation = Quaternion.Slerp(navigationArrow.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Allow drop-off when pressing "E"
        if (playerInDropoffZone && onRide && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Drop-off confirmed by player.");
            CompleteRide();
        }
    }



    void AssignRandomPickup()
    {
        pickupPoint = taxiPoints[Random.Range(0, taxiPoints.Count)];
        Debug.Log("Pickup assigned at: " + pickupPoint.Name);
    }

    void AssignDropoff()
    {
        do
        {
            dropoffPoint = taxiPoints[Random.Range(0, taxiPoints.Count)];
        } while (dropoffPoint == pickupPoint); // Ensure it's not the same as pickup

        estimatedTime = CalculateEstimatedTime(pickupPoint.TriggerZone.transform.position, dropoffPoint.TriggerZone.transform.position);
        startTime = Time.time;
        onRide = true;

        // Show waypoint marker for drop-off
        if (waypointMarker != null)
        {
            waypointMarker.transform.position = dropoffPoint.TriggerZone.transform.position + new Vector3(0, 2, 0);
            waypointMarker.SetActive(true);
        }

        Debug.Log("Dropoff assigned at: " + dropoffPoint.Name + " | Estimated time: " + estimatedTime);
    }



    float CalculateEstimatedTime(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        return distance / estimatedSpeed;
    }
    
void CompleteRide()
{
    float actualTime = Time.time - startTime;
    float fare = baseFare;

    string fareBreakdown = "Base Fare: " + baseFare;

    if (actualTime < estimatedTime)
    {
        fare *= speedBonusMultiplier;
        fareBreakdown += "\nSpeed Bonus: x" + speedBonusMultiplier;
    }
    else if (actualTime > estimatedTime + 10)
    {
        fare *= slowPenaltyMultiplier;
        fareBreakdown += "\nSlow Penalty: x" + slowPenaltyMultiplier;
    }

    fare *= comfortMultiplier * looksMultiplier;
    fareBreakdown += "\nComfort Multiplier: x" + comfortMultiplier;
    fareBreakdown += "\nLooks Multiplier: x" + looksMultiplier;

    Debug.Log("Ride complete! Fare: " + fare + "\n" + fareBreakdown);

    // Hide waypoint
    if (waypointMarker != null)
        waypointMarker.SetActive(false);

    // Reset everything
    pickupPoint = null;
    dropoffPoint = null;
    onRide = false;
    playerInDropoffZone = false;

    Debug.Log("Ready for next ride.");

    // Automatically assign a new pickup
    AssignRandomPickup();
}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; // Make sure only the player triggers this

        foreach (var point in taxiPoints)
        {
            if (other == point.TriggerZone)
            {
                if (pickupPoint == null) // Assign pickup if no ride is active
                {
                    pickupPoint = point;
                    Debug.Log("Picked up passenger at: " + point.Name);
                    AssignDropoff(); // Assign a drop-off point
                }
                else if (dropoffPoint == point && onRide) // If at drop-off, allow completion
                {
                    Debug.Log("Player reached drop-off zone: " + point.Name);
                    playerInDropoffZone = true;
                }
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && dropoffPoint != null && other == dropoffPoint.TriggerZone)
        {
            playerInDropoffZone = false;
            Debug.Log("Player left drop-off zone.");
        }
    }


}
