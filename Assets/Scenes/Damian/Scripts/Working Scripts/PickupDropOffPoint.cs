using UnityEngine;

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite pointSprite; // Assign in inspector for destination image
    public bool likesDriveBy;
    public bool likesDestruction;
    public PickUpCharacterAnimation passengerAnimation;  // Assign in inspector
}
