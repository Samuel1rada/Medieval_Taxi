using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public Canvas menu;

    public void Play()
    {
        SceneManager.LoadSceneAsync("Prototype");
        //SceneManager.LoadSceneAsync("pause");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Quitgame()
    {
        Application.Quit();
        Debug.Log("quit game");
    }

    public void Options()
    {
        menu.gameObject.SetActive(false);
    }
}
