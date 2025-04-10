using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class test : MonoBehaviour
{
    public int owned_cash = 0;  
    public int salary = 5;
    public float timeRemaining; 
    public float amountRemaining = 2;
    public bool timerIsRunning = false;

    public TextMeshProUGUI amount;   
    public bool onscreen = false;

    [SerializeField] public popup mypopup;

    public PickUpSystem pickUpSystem;

    void Start()
    {
        timeRemaining = amountRemaining;

        amount.text = "Money:" + owned_cash.ToString();

        OpenPopup();

        //mypopup.completed.onClick.AddListener(OpenClicked);  
        //OpenClicked();
                                                             
        mypopup.close.onClick.AddListener(CloseClicked);
    }

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.Space))
         {
            if (onscreen == false)
            {
                owned_cash += salary;
                mypopup.textMeshPro.text = "Cash gained: " + salary.ToString();
                amount.text = "Money:" + owned_cash.ToString();
                mypopup.gameObject.SetActive(true);  
                mypopup.animator.SetTrigger("fadein");
                Debug.Log($"New owned cash: {owned_cash}");
                timerIsRunning = true;
                onscreen = true;
            }
         }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime; // Reduce the time remaining
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = amountRemaining;
                timerIsRunning = false;
                //mypopup.gameObject.SetActive(false);
                onscreen = false;
                mypopup.animator.SetTrigger("fadeout");
            }
        }
    }

    private void OpenPopup()
    {
        mypopup.textMeshPro.text = "Current Cash: " + owned_cash.ToString();  
    }

    private void CloseClicked()
    {
        mypopup.gameObject.SetActive(false);  
        Debug.Log("closed");
    }
}
