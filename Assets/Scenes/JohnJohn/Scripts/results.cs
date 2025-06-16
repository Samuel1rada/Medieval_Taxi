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
        float current_score = ScoreManager.Instance.CurrentScore;
        totalScore.text = $"Final Score:" + current_score;

    }
}
