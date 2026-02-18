using UnityEngine;
using System.Collections;

public class RobotShutdownManager : MonoBehaviour
{
    public static RobotShutdownManager instance;

    [Header("Dependencies")]
    [SerializeField] private BatterySocketState socketState;
    [SerializeField] private RobotStartupManager startupManager;

    [Header("Shutdown Progress")]
    public bool remoteShutdownDone = false;
    public bool batteryClickedOff = false;
    public bool batteryExtracted = false;

    private void Awake() => instance = this;

    // --- 1. ADIM: KUMANDADAN KAPATMA (7. GÖREVİ TETİKLER) ---
    public void OnRemoteShutdownPressed()
    {
        // Önceki 6 görev tamamlandı mı kontrolü
        if (!ArePreviousTasksCompleted())
        {
            Debug.LogWarning("Önceki görevler tamamlanmadan shutdown yapılamaz!");
            return;
        }

        if (socketState.offRobot.activeSelf) return;

        // Robotu kapat (On -> Off model takası)
        socketState.SwapToOff();
        remoteShutdownDone = true;
        Debug.Log("Robot yazılımsal olarak kapatıldı. Şimdi fiziksel söküm başlasın.");
    }

    // --- 2. ADIM: BATARYAYA TIKLAMA (OFF KONUMU) ---
    // Bunu bataryanın üzerindeki tıklama scriptinden çağıracaksın
    public void OnBatteryClickedOff()
    {
        if (!remoteShutdownDone) return;
        batteryClickedOff = true;
        Debug.Log("Batarya anahtarı OFF konumuna getirildi.");
    }

    // --- 3. ADIM: BATARYAYI SOKETTEN ÇIKARMA ---
    // Bunu BatterySocketState veya batarya tutma/bırakma eventinden çağıracaksın
    public void OnBatteryExtracted()
    {
        if (!remoteShutdownDone) return;

        batteryExtracted = true;

        // BÜYÜK CEZA KONTROLÜ: Tıklamadan çıkardıysa
        if (!batteryClickedOff)
        {
            Debug.Log("<color=red>KRİTİK HATA: Batarya aktifken söküldü!</color>");
            GlobalDataManager.instance?.AddMistake("ARC FLASH DANGER!");
            GlobalDataManager.instance?.AddPoints(-1000); // Büyük ceza
            FindObjectOfType<ReportSlideController>()?.OpenReport();
        }
    }

    // --- FİNAL: ŞARJ İSTASYONUNA TAKMA VE KOL KONTROLÜ ---
    // Bu metod batarya şarj istasyonuna değdiği an tetiklenmeli
    public void CompleteShutdownTask()
    {
        if (!batteryExtracted) return;

        // Kol kontrolü (Ekstra puan için)
        if (startupManager.hasLoweredLever == false) // Yani kol yukarıdaysa (hasLoweredLever false ise)
        {
            Debug.Log("<color=green>Bonus: Kol güvenli konumda bırakıldı.</color>");
            GlobalDataManager.instance?.AddPoints(200);
        }
        else
        {
            Debug.Log("Bilgi: Kol indirilmiş (Açık) bırakıldı, bonus puan alınamadı.");
        }

        // Görevi bitir
        TaskUIManager.instance.CompleteTask("Shutdown");
        Debug.Log("7. GÖREV TAMAMLANDI: Robot güvenli bir şekilde kapatıldı.");
    }

    // 1'den 6'ya kadar olan görevlerin kontrolü
    private bool ArePreviousTasksCompleted()
    {
        if (TaskUIManager.instance == null) return false;

        string[] requiredIDs = { "Position", "LocateBattery", "InstallBattery", "StartRobot", "Animations", "Control30s" };
        
        foreach (string id in requiredIDs)
        {
            bool completed = false;
            foreach (var item in TaskUIManager.instance.tasks)
            {
                if (item.taskID == id && item.isCompleted)
                {
                    completed = true;
                    break;
                }
            }
            if (!completed) return false; // Eğer biri bile tamamlanmadıysa false dön
        }
        return true;
    }
}