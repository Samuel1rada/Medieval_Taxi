using UnityEngine;
using TMPro;

public class results : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI totalScore;
    [SerializeField] public TextMeshProUGUI rank;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float current_score = ScoreManager.Instance.CurrentScore;
        totalScore.text = $"Final Score:" + current_score;

        if (current_score >= 0)
        {
            rank.text = "Rank: D";
        }

        if (current_score >= 250)
        {
            rank.text = "Rank: C";
        }

        if (current_score >= 500)
        {
            rank.text = "Rank: B";
        }

        if (current_score >= 1000)
        {
            rank.text = "Rank: A";
        }
    }
}
