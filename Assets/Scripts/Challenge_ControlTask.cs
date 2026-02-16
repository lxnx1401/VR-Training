using UnityEngine;
using System.Collections;
using TMPro;

public class Challenge_ControlTask : MonoBehaviour
{
    [Header("Task Settings")]
    [SerializeField] private string taskID = "Control30s";
    [SerializeField] private float requiredTime = 30f;
    
    [Header("Robot References")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private string zapelBoolName = "zapel";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText; 

    private float currentTimer;
    private bool isTaskCompleted = false;
    private bool isZapelActive = false;
    private bool zapelTriggeredInThisTask = false;
    private float zapelAppearTime;
    private bool taskStarted = false;

    // YENİ: Ceza kilitleri (Görev boyunca sadece 1 kez ceza yesin diye)
    private bool hasTakenYellowPenalty = false;
    private bool hasTakenRedPenalty = false;

    void Start()
    {
        currentTimer = requiredTime;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isTaskCompleted || !taskStarted) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer < 0) currentTimer = 0;
        UpdateTimerUI();

        if (!zapelTriggeredInThisTask && currentTimer < (requiredTime * 0.6f)) 
        {
            TriggerZapel();
        }

        if (currentTimer <= 0 && !isZapelActive)
        {
            CompleteTask();
        }
    }

    public void StartMovementTask()
    {
        if (GlobalDataManager.instance != null && !GlobalDataManager.instance.isBatteryInstalled) return;
        if (isTaskCompleted) return;

        if (!taskStarted)
        {
            taskStarted = true;
        }

        // CEZA KONTROLÜ (Sadece daha önce ceza yenmediyse AreaSafetyManager'ı çağır)
        HandleSafetyPenalty();
    }

    private void HandleSafetyPenalty()
    {
        if (AreaSafetyManager.instance == null || AreaSafetyManager.instance.GetComponent<AreaSafetyManager>() == null) return;
        
        // AreaSafetyManager içindeki mantığı burada simüle edip kilit koyuyoruz
        // (SharedMaterial kontrolü AreaSafetyManager içinde olduğu için orayı tetikleriz ama kontrollü)
        
        // Önce bölgeyi bir kontrol edelim (AreaSafetyManager'ın bakacağı yerle aynı)
        // Eğer kırmızı bölgedeyse ve daha önce kırmızı cezası yemediyse:
        if (!hasTakenRedPenalty && IsInDangerZone("Red"))
        {
            AreaSafetyManager.instance.CheckSafetyAndPunish("Movement");
            hasTakenRedPenalty = true; // Artık bu görev boyunca kırmızı cezası yok
        }
        // Eğer sarı bölgedeyse ve daha önce ne sarı ne kırmızı cezası yemediyse (Kırmızı yiyen sarıyı da yemiş sayılmaz ama kafa karıştırmasın):
        else if (!hasTakenYellowPenalty && IsInDangerZone("Yellow"))
        {
            AreaSafetyManager.instance.CheckSafetyAndPunish("Movement");
            hasTakenYellowPenalty = true; // Artık bu görev boyunca sarı cezası yok
        }
    }

    // AreaSafetyManager'ın içine sızmadan materyali kontrol eden yardımcı fonksiyon
    private bool IsInDangerZone(string colorName)
    {
        // Not: Burada SafeZoneController'daki materyalleri kontrol ediyoruz
        // AreaSafetyManager üzerinden SafeZone referansını alıyoruz
        var sz = FindFirstObjectByType<SafeZoneController>(); 
        if (sz == null || sz.zoneRenderer == null) return false;

        Material currentMat = sz.zoneRenderer.sharedMaterial;
        if (colorName == "Red") return currentMat == sz.redMaterial;
        if (colorName == "Yellow") return currentMat == sz.yellowMaterial;
        return false;
    }

    private void TriggerZapel()
    {
        zapelTriggeredInThisTask = true;
        zapelAppearTime = Time.time;
        SetZapel(true);
    }

    public void NotifyEmergencyStop()
    {
        if (isTaskCompleted) return;

        if (isZapelActive)
        {
            float reactionTime = Time.time - zapelAppearTime;
            CalculateReactionScore(reactionTime);
            SetZapel(false);
        }
        else if (taskStarted)
        {
            GlobalDataManager.instance.AddMistake("PANIC STOP!");
            GlobalDataManager.instance.AddPoints(-50);
        }
    }

    private void CalculateReactionScore(float time)
    {
        if (GlobalDataManager.instance == null) return;
        if (time < 1.0f) GlobalDataManager.instance.AddPoints(200);
        else if (time < 2.5f) GlobalDataManager.instance.AddPoints(100);
        else 
        {
            GlobalDataManager.instance.AddMistake("LATE REACTION");
            GlobalDataManager.instance.AddPoints(-100);
        }
    }

    private void SetZapel(bool on)
    {
        isZapelActive = on;
        if (robotAnimator != null) robotAnimator.SetBool(zapelBoolName, on);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (!taskStarted) timerText.text = "PUSH BUTTONS TO START";
            else timerText.text = "TESTING... " + currentTimer.ToString("F1") + "s";
        }
    }

    private void CompleteTask()
    {
        isTaskCompleted = true;
        if (TaskUIManager.instance != null) TaskUIManager.instance.CompleteTask(taskID);
        if (timerText != null) timerText.text = "DRIVE TEST COMPLETED!";
    }
}