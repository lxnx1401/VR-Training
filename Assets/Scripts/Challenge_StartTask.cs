using UnityEngine;

public class Challenge_StartTask : MonoBehaviour
{
    // TaskUIManager'daki ID ile birebir aynı olmalı
    [SerializeField] private string taskID = "StartRobot"; 

    private bool hasTriggered = false;

    // Kumandadaki Start butonunun OnClick olayına bu fonksiyonu bağla aga
    public void ExecuteStartTask()
    {
        if (hasTriggered) return;

        // 1. Görevi Listeden Sildir (Üstünü çiz ve Tik at)
        if (TaskUIManager.instance != null)
        {
            TaskUIManager.instance.CompleteTask(taskID);
        }

        // 2. Global Veriyi Güncelle
        if (GlobalDataManager.instance != null)
        {
            // Zamanlayıcıyı başlat (Puan formülün için süre önemli)
            GlobalDataManager.instance.isTimerActive = true;
            
            // Eğer istersen ilk çalıştırma için ekstra bonus puan
            GlobalDataManager.instance.AddPoints(50); 
        }

        hasTriggered = true;
        Debug.Log("Challenge: Start görevi başarıyla tetiklendi ve listeye işlendi.");
    }
}