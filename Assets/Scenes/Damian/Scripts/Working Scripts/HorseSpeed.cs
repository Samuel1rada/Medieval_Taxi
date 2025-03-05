using MalbersAnimations.Controller;
using UnityEngine;

public class HorseSpeedController : MonoBehaviour
{
    public MAnimal horse;  // Assign the horse in the Inspector

    public float walkSpeed = 3f;
    public float trotSpeed = 6f;
    public float gallopSpeed = 12f;

    void Start()
    {
        if (horse != null)
        {
            // Modify Walk Speed
            MSpeed walk = horse.speedSets[0].Speeds[0];  // Get struct
            walk.Vertical = walkSpeed;                   // Modify value
            horse.speedSets[0].Speeds[0] = walk;          // Assign back

            // Modify Trot Speed
            MSpeed trot = horse.speedSets[0].Speeds[1];
            trot.Vertical = trotSpeed;
            horse.speedSets[0].Speeds[1] = trot;

            // Modify Gallop Speed
            MSpeed gallop = horse.speedSets[0].Speeds[2];
            gallop.Vertical = gallopSpeed;
            horse.speedSets[0].Speeds[2] = gallop;

            Debug.Log("Speed values updated successfully.");
        }
        else
        {
            Debug.LogError("Horse reference is missing!");
        }
    }
}
