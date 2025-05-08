using UnityEngine;

[System.Serializable]
public class PreferenceSettings
{
    [Header("Passenger Preferences")]
    public bool likesFast;
    public bool likesDriveBy;
    public bool likesDestruction;
    public bool likesRamps;
    [Range(0, 10)] public int preferredRampHeight = 3;

    [Header("NPC Characteristics")]
    public bool npcFast;
    public bool npcDriveBy;
    public bool npcDestruction;
    public bool npcRamps;
    [Range(0, 10)] public int npcRampHeight = 5;

    [Header("NPC Preferences About Player")]
    public bool likesPlayerFast;    // NPC prefers fast driving
    public bool likesPlayerDriveBy; // NPC prefers drive-by actions

    [Header("Player Characteristics")]
    public bool playerFast;         // Whether player is driving fast
    public bool playerDriveBy;      // Whether player does drive-bys
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