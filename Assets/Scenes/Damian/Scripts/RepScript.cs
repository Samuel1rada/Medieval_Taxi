using UnityEngine;
using TMPro;

public class RepScript : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    public int score = 0;

    // Update is called once per frame
    public void AddScore (int points)
    {
        score += points;
        UpdateScore();
    }

    void UpdateScore()
    {
        if (ScoreText != null)
        {
            ScoreText.text = "Score: " + score.ToString();
        }
    }


}
