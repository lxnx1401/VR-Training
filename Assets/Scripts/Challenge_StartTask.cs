using UnityEngine;

public class Challenge_StartTask : MonoBehaviour
{
    [SerializeField] private string taskID = "StartRobot"; 
    private bool hasTriggered = false;

    public void ExecuteStartTask()
    {
        // 1. KİLİT: Eğer batarya takılmadıysa hiçbir şey yapma
        if (GlobalDataManager.instance != null && !GlobalDataManager.instance.isBatteryInstalled)
        {
            Debug.LogWarning("Batarya takılmadan robot başlatılamaz!");
            return;
        }

        // 2. KİLİT: Eğer görev zaten bittiyse (hasTriggered), ne ceza kes ne puan ver
        if (hasTriggered) return;

        // Ceza kontrolü (Sadece ilk başarılı denemede veya hatalı denemede bir kez çalışır)
        if (AreaSafetyManager.instance != null)
        {
            AreaSafetyManager.instance.CheckSafetyAndPunish("StartRobot");
        }

        // Görevi bitir
        if (TaskUIManager.instance != null)
        {
            TaskUIManager.instance.CompleteTask(taskID);
        }

        if (GlobalDataManager.instance != null)
        {
            GlobalDataManager.instance.isTimerActive = true;
            GlobalDataManager.instance.AddPoints(50); 
        }

        hasTriggered = true;
        Debug.Log("Challenge: Start görevi başarıyla tamamlandı.");
    }
}