using UnityEngine;

public class IgnorePlayer : MonoBehaviour
{
    private void Start()
    {
   
        Physics.IgnoreLayerCollision(20, 22);
        
    }

}
