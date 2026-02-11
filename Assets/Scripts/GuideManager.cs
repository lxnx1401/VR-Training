using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GuideManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI guideTextField; // Metni buraya sürükle
    public GameObject nextButton;          // Butonu buraya sürükle

    [Header("Metin İçeriği")]
    [TextArea(3, 10)]
    public List<string> guideSentences; // Metinleri Inspector'dan ekleyeceğiz
    
    private int currentIndex = 0;

    void Start()
    {
        // İlk açıldığında ilk cümleyi göster
        ShowCurrentSentence();
    }

    public void NextSentence()
    {
        currentIndex++;

        if (currentIndex < guideSentences.Count)
        {
            ShowCurrentSentence();
        }
        else
        {
            // Rehber bittiğinde yapılacaklar
            FinishGuide();
        }
    }

    void ShowCurrentSentence()
    {
        guideTextField.text = guideSentences[currentIndex];
    }

    void FinishGuide()
    {
        guideTextField.text = "If you are ready click next to Start!.";
        // 2 saniye sonra paneli tamamen kapatmak istersen:
        Invoke("CloseCanvas", 3f);
    }

    public void CloseCanvas()
    {
        gameObject.SetActive(false); // Paneli kapat
    }

    public void OpenCanvas()
    {
        currentIndex = 0; // Başa sar
        gameObject.SetActive(true); // Paneli aç
        ShowCurrentSentence();
    }
}