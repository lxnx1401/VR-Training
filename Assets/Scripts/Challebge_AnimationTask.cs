using UnityEngine;
using System.Collections.Generic;

public class Challenge_AnimationTask : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string taskID = "Animations"; 
    [SerializeField] private int requiredCount = 2; 

    private HashSet<string> performedAnims = new HashSet<string>();
    private bool isDone = false;

    public void TrackAnimation(string animName)
    {
        // 1. KİLİT: Batarya yoksa animasyon sayma/ceza verme
        if (GlobalDataManager.instance != null && !GlobalDataManager.instance.isBatteryInstalled)
        {
            Debug.LogWarning("Batarya yokken robot hareket edemez!");
            return;
        }

        // 2. KİLİT: Görev bittiyse artık butonlar puan/ceza tetiklemesin
        if (isDone) return;

        // Sadece görev devam ediyorken ceza kontrolü yap
        if (AreaSafetyManager.instance != null)
        {
            AreaSafetyManager.instance.CheckSafetyAndPunish("Animations");
        }

        // Hareket sayacı
        if (!performedAnims.Contains(animName))
        {
            performedAnims.Add(animName);
            Debug.Log($"Hareket kaydedildi: {animName}. Toplam: {performedAnims.Count}/{requiredCount}");
        }

        if (performedAnims.Count >= requiredCount)
        {
            CompleteTask();
        }
    }

    private void CompleteTask()
    {
        isDone = true;
        if (TaskUIManager.instance != null)
        {
            TaskUIManager.instance.CompleteTask(taskID);
        }
        Debug.Log("Challenge: Animasyon görevi bitti, artık ceza/puan işlemez.");
    }
}