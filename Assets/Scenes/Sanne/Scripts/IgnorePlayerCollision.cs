using UnityEngine;

public class IgnorePlayer : MonoBehaviour
{
    private void Start()
    {
        Physics.IgnoreLayerCollision(1, 22);
        Physics.IgnoreLayerCollision(2, 22);
        Physics.IgnoreLayerCollision(20, 22);
        Physics.IgnoreLayerCollision(21, 22);
        
    }

}
