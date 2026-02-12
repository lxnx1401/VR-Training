using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject subtitleGroup;  // Altyazı grubu
    public GameObject pauseMenuGroup; // Menü grubu

    private bool isPaused = false;

    void Start()
    {
        // Oyun başında menü kapalı, altyazı açık başlasın
        subtitleGroup.SetActive(true);
        pauseMenuGroup.SetActive(false);
    }

    void Update()
    {
        // ESC'ye basınca menüyü aç/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        pauseMenuGroup.SetActive(isPaused);
        // Menü açıkken altyazı kapansın ki görüntü kirliliği olmasın
        subtitleGroup.SetActive(!isPaused);

        // VR'da oyunu gerçekten durdurmak istersen (isteğe bağlı):
        // Time.timeScale = isPaused ? 0f : 1f; 
    }
}