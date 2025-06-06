using UnityEngine;
using TMPro;

public class results : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI totalScore;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        totalScore.text = $"Final Score: {Mathf.RoundToInt(score_system._displayedScore)}";
    }
}
