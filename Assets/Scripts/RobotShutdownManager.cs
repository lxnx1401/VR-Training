using UnityEngine;
using UnityEngine.SceneManagement;

public class RobotShutdownManager : MonoBehaviour
{
    public static RobotShutdownManager instance;

    [Header("Dependencies")]
    [SerializeField] private BatterySocketState socketState;
    [SerializeField] private RobotStartupManager startupManager;

    [Header("Coordinate Settings (Inspector'dan Ayarla)")]
    [Tooltip("Off Robot'un bekleyeceği üst kat koordinatı")]
    public Vector3 offRobotWaitingPos;
    [Tooltip("Off Robot'un üst kattaki rotasyonu (Genelde 0,0,0)")]
    public Vector3 offRobotWaitingRot;

    [Header("Shutdown Progress")]
    public bool remoteShutdownDone = false;
    public bool batteryClickedOff = false;
    public bool batteryExtracted = false;

    private void Awake() => instance = this;

    // --- STARTUP KISMI: ROBOTU ÇALIŞTIRMA ---
    public void ExecuteSwapToOn()
    {
        if (socketState == null) return;

        // 1. ÖNCE: onRobot'u, offRobot'un şu anki tam konumuna getir
       socketState.onRobot.transform.position = socketState.offRobot.transform.position + new Vector3(0, -1f, 0);
        socketState.onRobot.transform.rotation = socketState.offRobot.transform.rotation * Quaternion.Euler(0, 180f, 0);

        // 2. SONRA: Off Robot'u üst kata ışınla
        socketState.offRobot.transform.position = offRobotWaitingPos;
        socketState.offRobot.transform.rotation = Quaternion.Euler(offRobotWaitingRot);

        // 3. VE SON: On Robot'u aktif et
        socketState.onRobot.SetActive(true);

        if (socketState.swapEffect != null) Instantiate(socketState.swapEffect, socketState.onRobot.transform.position, Quaternion.identity);
        Debug.Log("<color=cyan>On Robot, Off'un yerinde uyandı. Off üst kata uçtu.</color>");
    }

    // --- SHUTDOWN KISMI: KUMANDADAN KAPATMA ---
    public void OnRemoteShutdownPressed()
    {
        if (!ArePreviousTasksCompleted())
        {
            Debug.LogWarning("Önceki görevler bitmeden kapatılamaz!");
            return;
        }

        ExecuteSwapToOff();
        remoteShutdownDone = true;
    }

    public void ExecuteSwapToOff()
    {
        if (socketState == null || socketState.onRobot == null) return;

        // 1. On Robot'un o anki yerini al (Nereye götürdüysen)
        Vector3 currentPos = socketState.onRobot.transform.position;
        Quaternion currentRot = socketState.onRobot.transform.rotation;

        // 2. On Robot'u artık deaktif et (İşi bitti)
        socketState.onRobot.SetActive(false);

        // 3. Yukarıda bekleyen Off Robot'u onRobot'un olduğu yere indir
        // Senin istediğin 180 derece ters dönme olayını da ekledim
        socketState.offRobot.transform.position = currentPos + new Vector3(0, 1f, 0); // 1 metre yukarıda doğsun
        socketState.offRobot.transform.rotation = currentRot * Quaternion.Euler(0, 180f, 0);

        // --- OMUZ GARANTİLEME (HİLE) ---
        // Madem deaktif etmedik ama omuz hala çalışmazsa diye scripti bi' tıkla tazeliyoruz
        var omuzInteractable = socketState.offRobot.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (omuzInteractable != null)
        {
            omuzInteractable.enabled = false;
            omuzInteractable.enabled = true;
        }

        if (socketState.swapEffect != null) Instantiate(socketState.swapEffect, socketState.offRobot.transform.position, Quaternion.identity);
        Debug.Log("<color=orange>On Robot kapandı, Off Robot yukarıdan aşağıya indi.</color>");
    }

    // --- DİĞER GÖREV METODLARI (BOZMADIK) ---
    public void OnBatteryClickedOff() { if (remoteShutdownDone) batteryClickedOff = true; }
    
   // RobotShutdownManager.cs içinde şu metodu güncelle veya kontrol et:

public void OnBatteryExtracted()
{
    // Eğer kumandadan kapatma yapılmadıysa bu adımı sayma
    if (!remoteShutdownDone) return;

    batteryExtracted = true;
    Debug.Log("Batarya robotun gövdesinden başarıyla söküldü.");

    // KRİTİK HATA KONTROLÜ: 
    // Eğer batarya 'Off' konumuna getirilmeden (tıklanmadan) söküldüyse cezayı kes
    if (!batteryClickedOff)
    {
        Debug.Log("<color=red>KRİTİK HATA: Batarya aktifken (On) söküldü!</color>");
        if (GlobalDataManager.instance != null)
        {
            GlobalDataManager.instance.AddPoints(-1000); // Büyük ceza
            GlobalDataManager.instance.lastErrorName = "ARC FLASH DANGER - HOT SWAP!";
        }
        FindObjectOfType<ReportSlideController>()?.OpenReport();
    }
}

// Şarj istasyonuna takıldığında çağrılacak final metodu
public void CompleteShutdownTask()
{
    // Batarya önce sökülmüş olmalı
    if (!batteryExtracted) return;

    // Görevi listeden çiz
    if (TaskUIManager.instance != null)
    {
        TaskUIManager.instance.CompleteTask("Shutdown");
    }

    Debug.Log("<color=green>7. GÖREV TAMAMLANDI: Robot güvenli bir şekilde kapatıldı ve şarja takıldı.</color>");
    
    // 3 saniye sonra HighScore sahnesine git
    Invoke("GoToHighScoreScene", 3f);
}

    private void GoToHighScoreScene() => UnityEngine.SceneManagement.SceneManager.LoadScene("basicscene");

    private bool ArePreviousTasksCompleted()
    {
        if (TaskUIManager.instance == null) return false;
        string[] requiredIDs = { "Position", "LocateBattery", "InstallBattery", "StartRobot", "Animations", "Control30s" };
        foreach (string id in requiredIDs)
        {
            bool completed = false;
            foreach (var item in TaskUIManager.instance.tasks)
            {
                if (item.taskID == id && item.isCompleted) { completed = true; break; }
            }
            if (!completed) return false;
        }
        return true;
    }
}