using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class HUDController : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject pauseMenuGroup;
    public GameObject subtitleGroup;

    [Header("Input (Input System)")]
    public InputActionReference toggleMenuAction;

    [Header("Audio (Variante B)")]
    [Tooltip("Die AudioSource, die beim Pause-Menü pausiert werden soll (z.B. Dialog/Robot).")]
    public AudioSource dialogueAudio;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string skipSceneName = "Module02";

    private bool isPaused = false;

    private void OnEnable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnTogglePerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed -= OnTogglePerformed;
            toggleMenuAction.action.Disable();
        }
    }

    private void Start()
    {
        SetPaused(false);
    }

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        SetPaused(!isPaused);
    }

    private void SetPaused(bool pause)
    {
        isPaused = pause;

        Time.timeScale = isPaused ? 0f : 1f;

        // Variante B: nur diese AudioSource pausieren
        if (dialogueAudio != null)
        {
            if (isPaused) dialogueAudio.Pause();
            else dialogueAudio.UnPause();
        }

        if (pauseMenuGroup) pauseMenuGroup.SetActive(isPaused);
        if (subtitleGroup) subtitleGroup.SetActive(!isPaused);
    }

    // ===== Buttons im Pause-Menü =====

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Audio sicher wieder an
        if (dialogueAudio != null) dialogueAudio.UnPause();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SkipModule()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (dialogueAudio != null) dialogueAudio.UnPause();

        if (!string.IsNullOrEmpty(skipSceneName))
            SceneManager.LoadScene(skipSceneName);
        else
            Debug.LogWarning("[HUDController] skipSceneName ist leer.");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (dialogueAudio != null) dialogueAudio.UnPause();

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("[HUDController] mainMenuSceneName ist leer.");
    }

    public void Continue()
    {
        SetPaused(false);
    }
}
