using UnityEngine;

[System.Serializable]
public class PreferenceSettings
{
    [Header("Movement Preferences")]
    public bool likesFast = false;
    public bool likesDriveBy = false;

    [Header("Action Preferences")]
    public bool likesDestruction = false;
    public bool likesRamps = false;
    [Range(0, 10)] public int preferredRampHeight = 3;

    [Header("NPC Compatibility")]
    public bool npcFast = false;
    public bool npcDriveBy = false;
    public bool npcDestruction = false;
    public bool npcRamps = false;
    [Range(0, 10)] public int npcRampHeight = 5;
}

public class NPCPref : MonoBehaviour
{
    public enum PersonalityPreset
    {
        Custom,
        AdrenalineJunkie,
        CautiousDriver,
        DemolitionExpert,
        Balanced
    }

    [Header("Preset Selection")]
    public PersonalityPreset preset = PersonalityPreset.Custom;

    [Space(10)]
    [Header("Custom Preferences")]
    public PreferenceSettings preferences = new PreferenceSettings();

    // Public method to apply the selected preset
    public void ApplySelectedPreset()
    {
        if (preset == PersonalityPreset.Custom) return;

        switch (preset)
        {
            case PersonalityPreset.AdrenalineJunkie:
                preferences.likesFast = true;
                preferences.likesDriveBy = false;
                preferences.likesDestruction = true;
                preferences.likesRamps = true;
                preferences.preferredRampHeight = 9;
                preferences.npcFast = true;
                preferences.npcDestruction = true;
                preferences.npcRamps = true;
                preferences.npcRampHeight = 8;
                break;

            case PersonalityPreset.CautiousDriver:
                preferences.likesFast = false;
                preferences.likesDriveBy = false;
                preferences.likesDestruction = false;
                preferences.likesRamps = false;
                preferences.preferredRampHeight = 2;
                preferences.npcFast = false;
                preferences.npcDriveBy = false;
                preferences.npcDestruction = false;
                preferences.npcRamps = false;
                preferences.npcRampHeight = 3;
                break;

            case PersonalityPreset.DemolitionExpert:
                preferences.likesFast = false;
                preferences.likesDriveBy = false;
                preferences.likesDestruction = true;
                preferences.likesRamps = true;
                preferences.preferredRampHeight = 7;
                preferences.npcDestruction = true;
                preferences.npcRamps = true;
                preferences.npcRampHeight = 6;
                break;

            case PersonalityPreset.Balanced:
                preferences.likesFast = true;
                preferences.likesDriveBy = false;
                preferences.likesDestruction = false;
                preferences.likesRamps = false;
                preferences.preferredRampHeight = 5;
                preferences.npcFast = true;
                preferences.npcDriveBy = false;
                preferences.npcDestruction = false;
                preferences.npcRamps = false;
                preferences.npcRampHeight = 5;
                break;
        }
    }

    // This will automatically be called by Unity when values change in the inspector
    private void OnValidate()
    {
        ApplySelectedPreset();
    }

    public PreferenceSettings GetPreferences()
    {
        return preferences;
    }
}