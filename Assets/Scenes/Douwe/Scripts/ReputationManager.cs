using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReputationManager : MonoBehaviour
{
    [Header("Reputation")]
    [SerializeField] AnimationCurve reputationCurve;

    int currentLevel, totalReputation;
    int previousLevelsReputation, nextLevelsReputation;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI reputationText;
    [SerializeField] Image reputationFill;

    void Start()
    {
        UpdateLevel();
    }

    void Update() 
    {
        //if(Input.GetMouseButtonDown(0))
        //{
            //AddExperience(5);
        //}
    }

    public void AddReputation(int amount)
    {
        totalReputation += amount;
        CheckForLevelUp();
        UpdateInterface();
    }

    void CheckForLevelUp()
    {
        if(totalReputation >= nextLevelsReputation)
        {
            currentLevel++;
            UpdateLevel();

            // Start level up sequence... Possibly vfx?
        }
    }

    void UpdateLevel()
    {
        previousLevelsReputation = (int)reputationCurve.Evaluate(currentLevel);
        nextLevelsReputation = (int)reputationCurve.Evaluate(currentLevel + 1);
        UpdateInterface();
    }

    void UpdateInterface()
    {
        int start = totalReputation - previousLevelsReputation;
        int end = nextLevelsReputation - previousLevelsReputation; 

        levelText.text = currentLevel.ToString();
        reputationText.text = start + " exp / " + end + " exp";
        reputationFill.fillAmount = (float)start / (float)end;
    }
}
