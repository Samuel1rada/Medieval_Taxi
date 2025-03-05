using UnityEngine;
using UnityEngine.InputSystem;

public class MapMenu : MonoBehaviour
{
    public bool mapOpen = false;

    private PlayerInput playerInput;
    public Canvas Map;
    [SerializeField] private popup MapUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerInput = new PlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        if (mapOpen)
        {
            
        }
        else
        {
            
        }
    }


    private void OnMapcontrols()
    {
        if (mapOpen == false)
        {
            MapUI.animator.SetTrigger("fadein");
            //Map.gameObject.SetActive(true);
            mapOpen = true;
        }
        else
        {
            MapUI.animator.SetTrigger("fadeout");
            //Map.gameObject.SetActive(false);
            mapOpen = false;
        }
        Debug.Log("Map");
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }


}
