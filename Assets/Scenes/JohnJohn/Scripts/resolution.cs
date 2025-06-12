using UnityEngine;
using TMPro;

public class resolution : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DropdownResolution()
    {
        string index = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        switch (index)
        {
            case "0":
                Screen.SetResolution(1280, 720, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "1":
                Screen.SetResolution(1920, 1080, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "2":
                Screen.SetResolution(2560, 1440, true);
                Debug.Log(Screen.currentResolution);
                break;
            case "3":
                Screen.SetResolution(3840, 2160, true);
                Debug.Log(Screen.currentResolution);
                break;
        }
    }
}
