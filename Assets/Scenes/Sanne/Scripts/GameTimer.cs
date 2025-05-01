using UnityEngine;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    //Game timer
    public float targetTime = 600;

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
        }

    }

    //Happens when the timer ends
    void TimerEnded()
    {
        //Game pauze
        Time.timeScale = 0;
    }


}