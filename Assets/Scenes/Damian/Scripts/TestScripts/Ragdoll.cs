// using UnityEngine;
// using UnityEngine.UI;

// public class Ragdoll : MonoBehaviour
// {
//     [Header("NPC Ragdoll Settings")]
//     public bool autoFindRagdoll = true;
//     public GameObject npc;

//     [Header("Trigger Collider Settings")]
//     public Collider triggerCollider;

//     [Header("Ragdoll Deactivation Settings")]
//     public bool deactivateAfterExit = true;
//     public float deactivateDelay = 5f;

//     private Rigidbody[] npcRigidbodies;

//     private void Awake()
//     {
//         if (autoFindRagdoll)
//         {
//             if (npc == null)
//             {
//                 npc = gameObject;
//             }
//             npcRigidbodies = npc.GetComponentsInChildren<Rigidbody>();
//         }
//         else
//         {
//             if (npc == null)
//             {
//                 Debug.LogError("Assign the fokking NPC!");
//                 return;
//             }
//         }
//         DeactivateRagdoll();
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other == triggerCollider)
//         {
//             Debug.Log("Player entered the trigger");
//             ActivateRagdoll();
//         }
//     }
//     private void OnTriggerExit(Collider other)
//     {
//         if (other == triggerCollider)
//         {
//             Debug.Log("Player exited the trigger");
//             DeactivateRagdoll();
//         }
//     }
//     void ActivateRagdoll()
//     {
//         foreach (var rb in npcRigidbodies)
//         {
//             rb.isKinematic = false;
//         }
//     }


//     void DeactivateRagdoll()
//     {
//         foreach (var rb in npcRigidbodies)
//         {
//             rb.isKinematic = true;
//         }
//     }

// }
