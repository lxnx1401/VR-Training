using UnityEngine;
using TMPro;

public class ReportSlideController : MonoBehaviour
{
    // ... (Üst kısımdaki RectTransform ve Offset değişkenleri aynı kalsın) ...
    public RectTransform clipboardRect;
    public Vector2 closedOffset = new Vector2(-450, 0); 
    public Vector2 openedOffset = new Vector2(-750, 0); 
    public float slideSpeed = 10f;

    [Header("UI Text References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI lastErrorText; // "ERRORS:" yerine "LAST ERROR:"
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI rankText;

    private Vector2 baseAnchoredPos; 
    private bool isOpen = false;
    private bool initialized = false;

    void OnEnable()
    {
        if (!initialized && clipboardRect != null) { baseAnchoredPos = clipboardRect.anchoredPosition; initialized = true; }
        if (clipboardRect != null) { isOpen = false; clipboardRect.anchoredPosition = baseAnchoredPos + closedOffset; }
    }

    void Update()
    {
        if (clipboardRect == null) return;
        Vector2 targetPos = baseAnchoredPos + (isOpen ? openedOffset : closedOffset);
        clipboardRect.anchoredPosition = Vector2.Lerp(clipboardRect.anchoredPosition, targetPos, Time.unscaledDeltaTime * slideSpeed);
        if (isOpen) UpdateUI();
    }

    public void OpenReport() => isOpen = true;
    public void CloseReport() => isOpen = false;

    private void UpdateUI()
    {
        if (GlobalDataManager.instance == null) return;
        var data = GlobalDataManager.instance;

        if (scoreText != null) scoreText.text = "POINTS: " + data.currentScore;
        if (lastErrorText != null) lastErrorText.text = "LAST ERROR: " + data.lastErrorName;
        if (timeText != null) timeText.text = "TIME: " + FormatTime(data.sessionTimer);
        if (rankText != null) rankText.text = "RANK: " + GetRank();
    }

    string FormatTime(float t) => string.Format("{0:00}:{1:00}", Mathf.FloorToInt(t / 60), Mathf.FloorToInt(t % 60));

    // ASIL BOMBA BURASI: AKILLI RANK SİSTEMİ
    string GetRank()
    {
        var data = GlobalDataManager.instance;

        // 1. Durum: Çok fazla tuşa basıyorsa (Panik/Aceleci)
        if (data.totalInputs > 100 && data.totalMistakes > 5) return "PANICKED ROOKIE";
        
        // 2. Durum: Hiç hata yok ve puan çok yüksekse
        if (data.totalMistakes == 0 && data.currentScore > 1000) return "CYBER-SURGEON";

        // 3. Durum: Çok hızlı bitirdiyse (Zaman odaklı)
        if (data.sessionTimer < 120 && data.currentScore > 500) return "SONIC TECHNICIAN";

        // 4. Durum: Hata çoksa ama puan da varsa (Deneyerek öğrenen)
        if (data.totalMistakes > 10) return "RECKLESS REPAIRMAN";

        // 5. Durum: Standart Ranklar
        if (data.currentScore > 800) return "SENIOR MECHANIC";
        if (data.currentScore > 400) return "CERTIFIED TECH";
        if (data.currentScore <0) return "Omg plz delete the game bro";
        
        return "APPRENTICE";
    }
}
