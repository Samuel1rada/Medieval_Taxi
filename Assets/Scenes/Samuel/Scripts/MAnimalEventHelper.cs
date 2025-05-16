using MalbersAnimations.Controller;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MAnimalEventHelper : MonoBehaviour
{

    [SerializeField] MAnimal animal;
    [SerializeField] float delay = 1f;

    private bool isSprinting = false;
    private bool isSlowingDown = false;
    private float timer = 0f;
    public int minSpeedIndex = 1;

    private void Update()
    {
        if(isSprinting)
        {
            timer += Time.deltaTime;
            if (animal.CurrentSpeedIndex == 2)
            {
                animal.SpeedUp();
                timer = 0f;
            }
            else if(timer >= delay && animal.CurrentSpeedIndex == 3)
            {
                animal.SpeedUp();
                timer = 0f;
            }
        }
        if(isSlowingDown)
        {
            if (animal.CurrentSpeedIndex > minSpeedIndex)
            {
                timer += Time.deltaTime;
                if (timer >= delay)
                {
                    animal.SpeedDown();
                    timer = 0f;
                }
            }
        }
    }

    public void OnSprintpressed()
    {
        isSprinting = true;
        isSlowingDown = false;
        timer = 0f;
    }
    public void OnIsSprintReleased()
    {
        isSprinting = false;
        isSlowingDown = true;
        animal.SpeedDown();
        timer = 0f;
    }
}


