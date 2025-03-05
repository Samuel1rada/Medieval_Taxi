using UnityEngine;
using TMPro;
using MalbersAnimations.Controller;

public class Speedometer : MonoBehaviour
{
    public MAnimal horse;  // Assign your horse in the Inspector
    public TextMeshProUGUI speedText; // Assign a UI TextMeshPro element
    private Rigidbody rb; // Reference to the horse's Rigidbody

    void Start()
    {
        if (horse != null)
        {
            rb = horse.GetComponent<Rigidbody>(); // Get the Rigidbody component
        }
    }
// D
    void Update()
    {
        if (rb != null)
        {
            // Calculate the speed as the magnitude of the Rigidbody's velocity vector
            float speed = rb.linearVelocity.magnitude;

            // Update the UI text with the current speed
            speedText.text = "Speed: " + speed.ToString("F2") + " m/s";
        }
    }
}
