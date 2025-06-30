using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    [SerializeField] private float baseScore = 50f;
    [SerializeField] private float scoreCountDuration = 1.5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private RectTransform popupParent;

    [Header("Popup Settings")]
    [SerializeField] private Vector2 popupStartPosition = new Vector2(0, -100);
    [SerializeField] private float popupLifetime = 1.5f;
    [SerializeField] private float popupRiseDistance = 100f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    public float _currentScore = 0f;
    private float _displayedScore = 0f;
    private Coroutine _scoreCountingCoroutine;
    private Animator _scoreAnimator;

    public float CurrentScore
    {
        get => _currentScore;
        private set
        {
            _currentScore = Mathf.Max(0, value);
            StartScoreCountingAnimation();
            SaveScore();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
            ResetScore();
        }
    }

    private void InitializeComponents()
    {
        if (totalScoreText != null)
        {
            _scoreAnimator = totalScoreText.GetComponent<Animator>();
            if (_scoreAnimator == null)
            {
                Debug.LogWarning("Score text has no Animator - visual feedback will be limited");
            }
        }

        if (scorePopupPrefab == null) Debug.LogError("Score popup prefab is missing!");
        if (popupParent == null) Debug.LogError("Popup parent reference is missing!");
    }

    private void StartScoreCountingAnimation()
    {
        if (_scoreCountingCoroutine != null)
        {
            StopCoroutine(_scoreCountingCoroutine);
        }
        _scoreCountingCoroutine = StartCoroutine(CountToScore());
    }

    private IEnumerator CountToScore()
    {
        float startValue = _displayedScore;
        float endValue = _currentScore;
        float elapsed = 0f;

        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.deltaTime;
            _displayedScore = Mathf.Lerp(startValue, endValue, elapsed / scoreCountDuration);
            UpdateTotalScoreDisplay();
            yield return null;
        }

        _displayedScore = endValue;
        UpdateTotalScoreDisplay();
    }

    private void LoadScore()
    {
        CurrentScore = PlayerPrefs.GetFloat("TotalScore", 0f);
        _displayedScore = _currentScore;
        UpdateTotalScoreDisplay();
    }

    private void SaveScore()
    {
        PlayerPrefs.SetFloat("TotalScore", CurrentScore);
        PlayerPrefs.Save();
    }

    public void ResetScore()
    {
        CurrentScore = 0f;
        Debug.Log("Score reset to 0");
    }

    private void UpdateTotalScoreDisplay()
    {
        if (totalScoreText != null)
        {
            totalScoreText.text = $"SCORE: {Mathf.RoundToInt(_displayedScore)}";

            if (_scoreAnimator != null)
            {
                _scoreAnimator.Play("ScorePulse", 0, 0);
            }
        }
    }

    public void AddScore(float score)
    {
        if (Mathf.Approximately(score, 0f)) return;

        CurrentScore += score;
        ShowScorePopup(score);
    }

    private void ShowScorePopup(float score)
    {
        if (scorePopupPrefab == null || popupParent == null) return;

        GameObject popup = Instantiate(scorePopupPrefab, popupParent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchoredPosition = popupStartPosition;

        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (popupText != null)
        {
            string sign = score >= 0 ? "+" : "";
            popupText.text = $"{sign}{score:F0}";
            popupText.color = score >= 0 ? positiveColor : negativeColor;
            StartCoroutine(AnimatePopup(popup, popupText));
        }
        else
        {
            Destroy(popup);
        }
    }

    private IEnumerator AnimatePopup(GameObject popup, TextMeshProUGUI text)
    {
        float timer = 0f;
        RectTransform rect = popup.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;
        Color startColor = text.color;

        while (timer < popupLifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / popupLifetime;

            rect.anchoredPosition = Vector2.Lerp(
                startPos,
                startPos + (Vector2.up * popupRiseDistance),
                progress
            );

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

    public static float GetScoreForPreference(PreferenceLevel preference, float bonus, float penalty)
    {
        switch (preference)
        {
            case PreferenceLevel.Like:
                return bonus;
            case PreferenceLevel.Dislike:
                return -penalty;
            default:
                return 0f;
        }
    }

#if UNITY_EDITOR
    [Header("Editor Tools")]
    [SerializeField] private float testScoreAmount = 100f;

    [ContextMenu("Test Add Score")]
    private void TestAddScore()
    {
        AddScore(testScoreAmount);
    }

    [ContextMenu("Reset Score")]
    private void EditorResetScore()
    {
        ResetScore();
    }
#endif
}