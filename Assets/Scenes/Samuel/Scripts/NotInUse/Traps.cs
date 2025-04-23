using UnityEngine;

public class Traps : MonoBehaviour
{
    public SlipBehavior slipBehavior;

    public enum SlipBehavior
    {
        Spin,
        Slide,
        Freeze
    }

    private void OnCollisionStay(Collision collision)
    {
        // Optional: log which collider is touching
        Debug.Log($"{collision.collider.name} is touching the oil spill.");

        Transform root = collision.transform.root;
        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null || rb.isKinematic) continue;

            switch (slipBehavior)
            {
                case SlipBehavior.Spin:
                    rb.AddTorque(Vector3.up * 10f, ForceMode.Acceleration);
                    break;

                case SlipBehavior.Slide:
                    if (rb.linearVelocity.sqrMagnitude > 0.01f)
                        rb.AddForce(rb.linearVelocity.normalized * 3f, ForceMode.Acceleration);
                    break;

                case SlipBehavior.Freeze:
                    rb.linearVelocity = Vector3.zero;
                    break;
            }
        }
    }
}
