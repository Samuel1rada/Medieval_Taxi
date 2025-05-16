using MalbersAnimations.Controller;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MAnimalEventHelper : MonoBehaviour
{

    [SerializeField] private MAnimal animal;
    [SerializeField] private float delay = 1f;
    [SerializeField] private int minSpeedIndex = 2;

    private bool isSprinting = false;
    private bool isSlowingDown = false;
    private float timer = 0f;

    private void Update()
    {
        if (isSprinting)
        {
            timer += Time.deltaTime;

            if (animal.CurrentSpeedIndex == 2)
            {
                animal.SpeedUp();
                timer = 0f;
            }
            else if (timer >= delay && animal.CurrentSpeedIndex == 3)
            {
                animal.SpeedUp();
                timer = 0f;
            }
        }
        if (isSlowingDown)
        {
            if (animal.CurrentSpeedIndex > minSpeedIndex)
            {
                timer += Time.deltaTime;

                if (timer >= delay)
                {
                    timer = 0f;

                    if (animal.CurrentSpeedIndex <= minSpeedIndex)
                    {
                        isSlowingDown = false;
                        Debug.Log("Reached minimum speed index. Stopping slowdown.");
                        return;
                    }

                    animal.SpeedDown();

                }
                Debug.Log("Slowing down: Current Speed Index = " + animal.CurrentSpeedIndex);
            }
            else
            {
                isSlowingDown = false;
                timer = 0f;
                Debug.Log("Already at or below minimum speed index. No slowdown needed.");
            }
        }
    }

    public void OnSprintpressed()
    {
        isSprinting = true;
        isSlowingDown = false;
        timer = 0f;
        Debug.Log("Sprint started.");
    }

    public void OnIsSprintReleased()
    {
        isSprinting = false;
        isSlowingDown = true;
        timer = 0f;
        Debug.Log("Sprint released — slowing down initiated.");
    }
}


