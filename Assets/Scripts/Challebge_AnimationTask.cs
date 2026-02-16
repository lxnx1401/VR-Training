using UnityEngine;
using System.Collections.Generic;

public class Challenge_AnimationTask : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string taskID = "Animations"; // TaskUIManager'daki ID ile aynı
    [SerializeField] private int requiredCount = 2; // Arkadaşının istediği gibi 2 farklı hareket

    private HashSet<string> performedAnims = new HashSet<string>();
    private bool isDone = false;

    // Bu fonksiyonu butonlara bağlayacağız
    public void TrackAnimation(string animName)
    {
        if (isDone) return;

        // Daha önce yapılmamış bir hareketse listeye ekle
        if (!performedAnims.Contains(animName))
        {
            performedAnims.Add(animName);
            Debug.Log("Challenge: Hareket kaydedildi: " + animName);
        }

        // Hedefe ulaşıldı mı?
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
        Debug.Log("Challenge: Animasyon görevi tamamlandı!");
    }
}