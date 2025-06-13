using UnityEngine;

public enum PreferenceLevel
{
    Dislike,
    Neutral,
    Like
}

[System.Serializable]
public class PickupDropoffPoint
{
    public string pointName;
    public Transform pointTransform;
    public Sprite pointSprite; // Assign in inspector for destination image
    public PreferenceLevel driveByPreference;      // Use enum for drive-by
    public PreferenceLevel destructionPreference;  // Use enum for destruction
    public PickUpCharacterAnimation passengerAnimation;  // Assign in inspector
}
