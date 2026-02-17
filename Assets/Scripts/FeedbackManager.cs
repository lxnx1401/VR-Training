using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI feedbackText; // Sağ üstteki metin
    public RectTransform textTransform; // Animasyon için transform

    [Header("Ses Efektleri")]
    public AudioSource audioSource;
    public AudioClip positiveSound;
    public AudioClip negativeSound;

    [Header("Ayarlar")]
    public float displayDuration = 1.5f; // Ekranda kalma süresi
    public float moveDistance = 50f;     // Yukarı doğru kayma miktarı

    private int lastScore = 0;
    private Vector2 originalPos;
    private Coroutine currentCoroutine;

    void Start()
    {
        if (feedbackText != null)
        {
            originalPos = textTransform.anchoredPosition;
            feedbackText.text = ""; // Başlangıçta boş
        }

        if (GlobalDataManager.instance != null)
        {
            lastScore = GlobalDataManager.instance.currentScore;
        }
    }

    void Update()
    {
        if (GlobalDataManager.instance == null) return;

        int currentScore = GlobalDataManager.instance.currentScore;

        // Puan değişmiş mi kontrol et
        if (currentScore != lastScore)
        {
            int difference = currentScore - lastScore;
            ShowFeedback(difference);
            lastScore = currentScore;
        }
    }

    private void ShowFeedback(int diff)
    {
        // Eğer zaten bir yazı varsa onu durdurup yenisini başlat
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        
        currentCoroutine = StartCoroutine(AnimateFeedback(diff));
    }

    private IEnumerator AnimateFeedback(int diff)
    {
        // Renk ve İçerik Ayarı
        if (diff > 0)
        {
            feedbackText.text = "+" + diff;
            feedbackText.color = Color.green;
            if (positiveSound != null) audioSource.PlayOneShot(positiveSound);
        }
        else
        {
            feedbackText.text = diff.ToString(); // Zaten başında eksi var
            feedbackText.color = Color.red;
            if (negativeSound != null) audioSource.PlayOneShot(negativeSound);
        }

        // Animasyon: Yukarı Kayma ve Kaybolma (Fade Out)
        float elapsed = 0;
        Color startColor = feedbackText.color;

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / displayDuration;

            // Yukarı kaydır
            textTransform.anchoredPosition = originalPos + new Vector2(0, t * moveDistance);

            // Opaklığı düşür (Fade out)
            feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, t));

            yield return null;
        }

        feedbackText.text = "";
        textTransform.anchoredPosition = originalPos;
    }
}