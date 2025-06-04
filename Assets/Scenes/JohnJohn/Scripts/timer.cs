using UnityEngine;
using TMPro;

public class timer : MonoBehaviour
{
    //Game timer
    public float targetTime = 600;

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Canvas timeOutCanvas;

    private void Start()
    {
        timeOutCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (targetTime > 0)
        {
            targetTime -= Time.deltaTime;
        }

        if (targetTime < 0)
        {
            targetTime = 0;
            TimerEnded();
            timeOutCanvas.gameObject.SetActive(true);
        }

        if (targetTime < 60)
        {
            timerText.color = Color.red;
        }

        int minutes = Mathf.FloorToInt(targetTime / 60);
        int seconds = Mathf.FloorToInt(targetTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }

    //Happens when the timer ends
    void TimerEnded()
    {
        //Game pauze
        Time.timeScale = 0;
        pause.pausable = false;
        pause.isPaused = false;
    }
}
