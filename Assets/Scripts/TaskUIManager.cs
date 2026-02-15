using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskUIManager : MonoBehaviour
{
    public static TaskUIManager instance;

    [System.Serializable]
    public class TaskUIItem
    {
        public string taskID;         // Buraya "Position", "LocateBattery" gibi kısa isimler yaz
        public TextMeshProUGUI taskText; 
        public Image checkIcon; 
        [HideInInspector] public bool isCompleted = false;
    }

    public TaskUIItem[] tasks;

    // Görev ID'lerini tam cümleye çeviren sözlük
    private Dictionary<string, string> taskDescriptions = new Dictionary<string, string>()
    {
        { "Position", "1. Position Robot in SafeZone" },
        { "LocateBattery", "2. Find the Battery!" },
        { "InstallBattery", "3. Install the Battery" },
        { "StartRobot", "4. Activate Robot via Remote" },
        { "Animations", "5. Test Movement Functions" },
        { "Emergency", "6. Safety Check: Emergency Stop" }
    };

    void Awake() { instance = this; }

    void Start()
    {
        // Oyun başladığında yazıların içini otomatik doldur
        foreach (var item in tasks)
        {
            if (item.taskText != null && taskDescriptions.ContainsKey(item.taskID))
            {
                item.taskText.text = taskDescriptions[item.taskID];
                item.taskText.fontStyle = FontStyles.Normal; // Başlangıçta normal
            }

            // Tikleri başta gizle
            if (item.checkIcon != null) item.checkIcon.enabled = false;
        }
    }

    public void CompleteTask(string id)
    {
        foreach (var item in tasks)
        {
            if (item.taskID == id && !item.isCompleted)
            {
                item.isCompleted = true;
                
                // Metni gri yap ve üstünü çiz
                item.taskText.fontStyle = FontStyles.Strikethrough;
                item.taskText.color = Color.gray;
                
                // Tik kutusunu aç
                if (item.checkIcon != null) item.checkIcon.enabled = true;
                
                // Puan ver
                if (GlobalDataManager.instance != null)
                    GlobalDataManager.instance.AddPoints(100);
                
                break;
            }
        }
    }
}
