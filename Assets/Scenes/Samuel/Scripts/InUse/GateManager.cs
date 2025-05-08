using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField] private Animator gateOpen;
    private Collider gateCollider;
    private void Awake()
    {
        if (gateCollider == null)
        {
            gateCollider = GetComponent<Collider>();
        }
            

        gateCollider.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("N was pressed");
            gateCollider.enabled = true;
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
