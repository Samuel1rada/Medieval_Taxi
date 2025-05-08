using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class pause : MonoBehaviour
{

    public bool isPaused = false;
    public Canvas pause_menu;

    [SerializeField] private Animator PauseUi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pause_menu.gameObject.SetActive(false);
        Go();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPause()
    {
        if (isPaused == false)
        {
            Stop();
        }
        else
        {
            Go();
        }
        
    }

    public void Stop()
    {
        pause_menu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Go()
    {
        pause_menu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene("prototype");
    }

    public void Quitgame()
    {
        SceneManager.LoadSceneAsync("main_menu");
        Debug.Log("quit game");
    }

}
