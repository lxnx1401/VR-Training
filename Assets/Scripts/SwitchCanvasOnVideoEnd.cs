// SwitchCanvasOnVideoEnd.cs
using UnityEngine;
using UnityEngine.Video;

public class SwitchCanvasOnVideoEnd : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Canvases")]
    [SerializeField] private GameObject frontCanvas;   // Canvas mit Video
    [SerializeField] private GameObject nextCanvas;    // Canvas danach

    [Header("Optional")]
    [SerializeField] private bool startVideoOnEnable = true;

    private void Awake()
    {
        if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();

        if (!videoPlayer)
        {
            Debug.LogError("[SwitchCanvasOnVideoEnd] Kein VideoPlayer zugewiesen oder am gleichen Objekt vorhanden!");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // Sicherstellen, dass am Anfang der richtige Canvas-Status aktiv ist
        if (frontCanvas) frontCanvas.SetActive(true);
        if (nextCanvas) nextCanvas.SetActive(false);

        videoPlayer.loopPointReached += OnVideoFinished;

        if (startVideoOnEnable)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (frontCanvas) frontCanvas.SetActive(false);
        if (nextCanvas) nextCanvas.SetActive(true);
    }
}
