using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class TapeManager : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public List<AudioClip> playlist;
    private int currentTrackIndex = 0;

    [Header("UI Elemanları")]
    public TextMeshProUGUI trackNameText;
    public Image playPauseButtonImage; 
    public Sprite playIcon;  
    public Sprite pauseIcon; 
    public float scrollSpeed = 50f;
    public float resetPositionX = 300f; // Yazının sağdan başlama noktası
    public float exitPositionX = -300f;  // Yazının soldan kaybolma noktası

    [Header("Zıplama Efekti")]
    public Transform robotBody; 
    public float bounceIntensity = 0.05f;
    public float bounceSpeed = 10f;
    private Vector3 originalPosition;

    private bool isPlaying = false;

    void Start()
    {
        if (robotBody != null)
            originalPosition = robotBody.localPosition;

        // Sahne açıldığında ilk şarkı ismini yazdır
        if (playlist.Count > 0) 
        {
            UpdateTrackUI();
        }
        
        // Ses ayarlarını kodla sağlama alalım
        if (audioSource != null)
            audioSource.spatialBlend = 1.0f; 

        // Başlangıç ikonu
        if(playPauseButtonImage != null && playIcon != null) 
            playPauseButtonImage.sprite = playIcon;
    }

    void Update()
    {
        // --- 1. HER ZAMAN KAYAN YAZI ---
        if (playlist.Count > 0 && trackNameText != null)
        {
            trackNameText.rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // Yazı sınırdan çıkınca sağdan tekrar girsin
            if (trackNameText.rectTransform.anchoredPosition.x < exitPositionX) 
                trackNameText.rectTransform.anchoredPosition = new Vector2(resetPositionX, trackNameText.rectTransform.anchoredPosition.y);
        }

        // --- 2. ZIPLAMA MANTIĞI (SADECE ÇALARKEN) ---
        if (isPlaying && robotBody != null)
        {
            // Mathf.Abs sayesinde sadece yukarı zıplar, yerin içine girmez
            float bounce = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * bounceIntensity;
            robotBody.localPosition = originalPosition + new Vector3(0, bounce, 0);
        }
        else if (robotBody != null)
        {
            // Durduğunda yumuşakça yerine otur
            robotBody.localPosition = Vector3.Lerp(robotBody.localPosition, originalPosition, Time.deltaTime * 5f);
        }
    }

    public void PlayPauseButton()
    {
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
            if(playPauseButtonImage != null) playPauseButtonImage.sprite = playIcon;
        }
        else
        {
            // Eğer daha önce hiç çalmadıysa ilk şarkıyı yükle
            if (audioSource.clip == null && playlist.Count > 0) 
                audioSource.clip = playlist[currentTrackIndex];
            
            audioSource.Play();
            isPlaying = true;
            if(playPauseButtonImage != null) playPauseButtonImage.sprite = pauseIcon;
        }
    }

    public void StopButton()
    {
        audioSource.Stop();
        isPlaying = false;
        if(playPauseButtonImage != null) playPauseButtonImage.sprite = playIcon;
        // Yazı akmaya devam edecek, ismine dokunmuyoruz.
    }

    public void NextTrack()
    {
        if (playlist.Count == 0) return;
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayTrack();
    }

    public void PrevTrack()
    {
        if (playlist.Count == 0) return;
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = playlist.Count - 1;
        PlayTrack();
    }

    private void PlayTrack()
    {
        audioSource.clip = playlist[currentTrackIndex];
        audioSource.Play();
        isPlaying = true;
        if(playPauseButtonImage != null) playPauseButtonImage.sprite = pauseIcon;
        UpdateTrackUI();
    }

    private void UpdateTrackUI()
    {
        if (trackNameText != null && playlist.Count > 0)
        {
            trackNameText.text = playlist[currentTrackIndex].name;
            // Şarkı değiştiğinde yazıyı sağa at ki kayma baştan başlasın
            trackNameText.rectTransform.anchoredPosition = new Vector2(resetPositionX, trackNameText.rectTransform.anchoredPosition.y);
        }
    }
}