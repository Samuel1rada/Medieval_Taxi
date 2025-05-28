using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class pause : MonoBehaviour
{

    public bool isPaused = false;
    public bool pausable = true;
    public Canvas pause_menu;
    [SerializeField] private Animator PauseUi;
    public Canvas options_ui;

    void Start()
    {
        pause_menu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausable)
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }

        }
    }

    public void Pause()
    {
        pause_menu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused");
    }

    public void Resume()
    {
        pause_menu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("prototype");
    }

    public void Quitgame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadSceneAsync("main_menu");
        Debug.Log("Quit game to main menu");
    }

    public void Options()
    {
        pause_menu.gameObject.SetActive(false);
        options_ui.gameObject.SetActive(true);
    }

    public void Backoptions()
    {
        pause_menu.gameObject.SetActive(true);
        options_ui.gameObject.SetActive(false);
    }
}
