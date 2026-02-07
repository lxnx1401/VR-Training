using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Button kann diese Funktion mit Szenen-Namen aufrufen
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // Optional: per Build Index laden
    public void LoadSceneByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Invalid build index: {buildIndex}");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    // Optional: Quit (für Builds)
    public void QuitApp()
    {
        Application.Quit();
    }
}
