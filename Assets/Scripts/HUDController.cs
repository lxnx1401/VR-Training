using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class HUDController : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject pauseMenuGroup; 
    public GameObject subtitleGroup;  

    [Header("Input")]
    public InputActionReference pauseAction;

   
   

    private bool isPaused = false;

    void Start()
    {
       
        
        ResumeGame();
    }

    void Update()
    {
        bool pcPress = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool vrPress = pauseAction != null && pauseAction.action.triggered;

        if (pcPress || vrPress)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        
        pauseMenuGroup.SetActive(true);
        subtitleGroup.SetActive(false);

        
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        
        pauseMenuGroup.SetActive(false);
        subtitleGroup.SetActive(true);
        
        
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() => Application.Quit();
}