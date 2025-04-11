using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField] private Animator gateOpen;
    [SerializeField] private BoxCollider collider;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
        collider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            collider.enabled = true;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            gateOpen.SetBool("GateOpen", true);
        }
       
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gateOpen.SetBool("GateOpen", false);
        }

    }

}
