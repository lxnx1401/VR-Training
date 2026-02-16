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

    void Start()
    {
        currentTimer = requiredTime;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isTaskCompleted || !taskStarted) return;

        // Görev başladıysa süreyi düşür
        currentTimer -= Time.deltaTime;
        if (currentTimer < 0) currentTimer = 0;
        UpdateTimerUI();

        // RASTGELE ARIZA TETİKLEME
        if (!zapelTriggeredInThisTask && currentTimer < (requiredTime * 0.6f)) 
        {
            TriggerZapel();
        }

        if (currentTimer <= 0 && !isZapelActive)
        {
            CompleteTask();
        }
    }

    // AGA KRİTİK NOKTA BURASI: Bunu hareket butonlarına bağlayacaksın
    public void StartMovementTask()
    {
        if (!taskStarted)
        {
            taskStarted = true;
            Debug.Log("Challenge: İlk hareket algılandı, süre başlıyor!");
        }
    }

    private void TriggerZapel()
    {
        zapelTriggeredInThisTask = true;
        zapelAppearTime = Time.time;
        SetZapel(true);
    }

    public void NotifyEmergencyStop()
    {
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
        else {
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