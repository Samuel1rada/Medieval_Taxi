using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    [SerializeField] private float baseScore = 50f;
    [SerializeField] private float preferenceMultiplier = 1.5f;
    [SerializeField] private float rampHeightWeight = 2f;
    [SerializeField] private float perfectHeightBonus = 25f;
    private float currentScore = 0f;

    [Header("Popup Settings")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private RectTransform popupParent;
    [SerializeField] private Vector2 popupStartPosition = new Vector2(0, -100);
    [SerializeField] private float popupLifetime = 1.5f;
    [SerializeField] private float popupRiseDistance = 100f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(float score)
    {
        currentScore += score;
        ShowScorePopup(score);
        Debug.Log($"Score changed: {score}. Total: {currentScore}");
    }

    private void ShowScorePopup(float score)
    {
        if (scorePopupPrefab == null || popupParent == null)
        {
            Debug.LogWarning("Popup references not assigned!");
            return;
        }

        GameObject popup = Instantiate(scorePopupPrefab, popupParent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchoredPosition = popupStartPosition;

        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (popupText == null)
        {
            Debug.LogError("Popup prefab missing TextMeshPro component!");
            Destroy(popup);
            return;
        }

        string sign = score >= 0 ? "+" : "";
        popupText.text = $"{sign}{score:F0}";
        popupText.color = score >= 0 ? positiveColor : negativeColor;

        StartCoroutine(AnimatePopup(popup));
    }

    private IEnumerator AnimatePopup(GameObject popup)
    {
        float timer = 0f;
        RectTransform rect = popup.GetComponent<RectTransform>();
        TextMeshProUGUI text = popup.GetComponentInChildren<TextMeshProUGUI>();
        Vector2 startPos = rect.anchoredPosition;
        Color startColor = text.color;

        while (timer < popupLifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / popupLifetime;

            // Movement
            rect.anchoredPosition = Vector2.Lerp(
                startPos,
                startPos + Vector2.up * popupRiseDistance,
                progress
            );

            // Fading
            if (progress > 0.6f)
            {
                text.color = new Color(
                    startColor.r,
                    startColor.g,
                    startColor.b,
                    Mathf.Lerp(1f, 0f, (progress - 0.6f) / 0.4f)
                );
            }

            yield return null;
        }

        Destroy(popup);
    }

    public float CalculateCompatibilityScore(PreferenceSettings passenger, PreferenceSettings npc)
    {
        float score = baseScore;

        if (passenger.likesFast && npc.npcFast)
            score *= ApplyPreferenceMultiplier("Speed", passenger.likesFast, npc.npcFast);

        if (passenger.likesDriveBy && npc.npcDriveBy)
            score *= ApplyPreferenceMultiplier("DriveBy", passenger.likesDriveBy, npc.npcDriveBy);

        if (passenger.likesDestruction && npc.npcDestruction)
            score *= ApplyPreferenceMultiplier("Destruction", passenger.likesDestruction, npc.npcDestruction);

        if (passenger.likesRamps && npc.npcRamps)
        {
            score *= ApplyPreferenceMultiplier("Ramps", passenger.likesRamps, npc.npcRamps);
            score += CalculateRampBonus(passenger.preferredRampHeight, npc.npcRampHeight);
        }

        return Mathf.Max(0, score);
    }

    private float ApplyPreferenceMultiplier(string preferenceName, bool passengerLikes, bool npcHas)
    {
        if (passengerLikes && npcHas)
        {
            Debug.Log($"Positive match for {preferenceName}");
            return preferenceMultiplier;
        }
        return 1f;
    }

    private float CalculateRampBonus(int passengerHeight, int npcHeight)
    {
        float heightDifference = Mathf.Abs(passengerHeight - npcHeight);
        float heightSimilarity = 1f - (heightDifference / 10f);

        if (heightDifference == 0)
        {
            Debug.Log("Perfect ramp height match!");
            return perfectHeightBonus;
        }

        return heightSimilarity * rampHeightWeight;
    }

    public void ShowFinalScore(PreferenceSettings passengerPrefs, PreferenceSettings journeyStats, DeliverySpeed speed)
    {
        float baseScore = CalculateCompatibilityScore(passengerPrefs, journeyStats);
        float speedMultiplier = GetSpeedMultiplier(speed);
        float finalScore = baseScore * speedMultiplier;
        
        AddScore(finalScore);
    }

    private float GetSpeedMultiplier(DeliverySpeed speed)
    {
        switch (speed)
        {
            case DeliverySpeed.Fast: return 1.5f;
            case DeliverySpeed.Medium: return 1.0f;
            case DeliverySpeed.Slow: return 0.7f;
            default: return 1.0f;
        }
    }

    public void HandleRampUsed(int rampHeight, PreferenceSettings passenger)
    {
        float scoreChange = passenger.likesRamps 
            ? CalculateRampBonus(passenger.preferredRampHeight, rampHeight)
            : -rampHeightWeight * 2f;

        AddScore(scoreChange);
    }

    public enum DeliverySpeed { Slow, Medium, Fast }

    // For debugging
    [ContextMenu("Test Score Popup")]
    private void TestPopup()
    {
        AddScore(100);
    }
}