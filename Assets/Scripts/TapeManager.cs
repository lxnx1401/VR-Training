using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI; // Image bileşeni için şart

public class TapeManager : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public List<AudioClip> playlist;
    private int currentTrackIndex = 0;

    [Header("UI Elemanları")]
    public TextMeshProUGUI trackNameText;
    public Image playPauseButtonImage; // Butonun "Source Image"ı buraya gelecek
    public Sprite playIcon;  // Play (Üçgen) ikonu
    public Sprite pauseIcon; // Pause (İki Çizgi) ikonu
    public float scrollSpeed = 50f;

    [Header("Zıplama Efekti")]
    public Transform robotBody; 
    public float bounceIntensity = 0.05f;
    public float bounceSpeed = 10f;
    private Vector3 originalPosition;

    private bool isPlaying = false;

    void Start()
    {
        originalPosition = robotBody.localPosition;
        if (playlist.Count > 0) UpdateTrackUI();
        
        audioSource.spatialBlend = 1.0f; 
        // Başlangıçta ikon Play olsun
        if(playPauseButtonImage != null && playIcon != null) 
            playPauseButtonImage.sprite = playIcon;
    }

    void Update()
    {
        if (isPlaying)
        {
            trackNameText.rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
            if (trackNameText.rectTransform.anchoredPosition.x < -300) 
                trackNameText.rectTransform.anchoredPosition = new Vector2(300, trackNameText.rectTransform.anchoredPosition.y);

            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceIntensity;
            robotBody.localPosition = originalPosition + new Vector3(0, bounce, 0);
        }
        else
        {
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
            if (audioSource.clip == null) audioSource.clip = playlist[currentTrackIndex];
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
        trackNameText.rectTransform.anchoredPosition = new Vector2(0, trackNameText.rectTransform.anchoredPosition.y);
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayTrack();
    }

    public void PrevTrack()
    {
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
        trackNameText.text = playlist[currentTrackIndex].name;
        trackNameText.rectTransform.anchoredPosition = new Vector2(250, trackNameText.rectTransform.anchoredPosition.y);
    }
}