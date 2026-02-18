using UnityEngine;

public class RobotStartupManager : MonoBehaviour
{
    public static RobotStartupManager instance;

    [Header("Dependencies")]
    [SerializeField] private BatterySocketState socketState;
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private string leverBoolName = "Pressed";

    [Header("Current Progress")]
    public bool hasLiftedLever = false;
    public bool hasInsertedBattery = false;
    public bool hasClickedBattery = false;
    public bool hasLoweredLever = false;

    private void Awake() => instance = this;

    // --- ADIM TAKİPLERİ ---
    public void SetLeverState(bool currentState)
    {
        if (currentState == true) 
        {
            hasLiftedLever = true;
            hasLoweredLever = false; 
            Debug.Log("Kol yukarıda.");
        }
        else 
        {
            hasLoweredLever = true;
            Debug.Log("Kol aşağıda.");
        }
    }

    // --- START TUŞU FONKSİYONU ---
    public void OnStartButtonPressed()
    {
        if (socketState == null || !socketState.IsBatteryInSocket)
        {
            Debug.LogWarning("Batarya yok! Robot çalıştırılamaz.");
            return; 
        }

        if (!hasClickedBattery)
        {
            Debug.LogWarning("Batarya takılı ama aktif değil (Tıklanmadı)!");
            return;
        }

        // --- DEĞİŞİKLİK BURADA: Artık ShutdownManager'daki ışınlama metodunu çağırıyoruz ---
        if (socketState.offRobot != null)
        {
            // Eskiden socketState.SwapRobots() idi, şimdi yeni sisteme bağladık:
            if (RobotShutdownManager.instance != null)
            {
                RobotShutdownManager.instance.ExecuteSwapToOn();
            }
            else
            {
                Debug.LogError("RobotShutdownManager bulunamadı! Işınlama yapılamıyor.");
            }
            
            if (TaskUIManager.instance != null)
            {
                TaskUIManager.instance.CompleteTask("StartRobot");
            }
        }

        CheckProcedureAndScore();
    }

    private void CheckProcedureAndScore()
    {
        if (hasLoweredLever)
        {
            Debug.Log("<color=green>Başarılı çalıştırma!</color>");
        }
        else
        {
            Debug.Log("<color=red>Hata: Kol indirilmedi!</color>");
            if (GlobalDataManager.instance != null)
            {
                GlobalDataManager.instance.lastErrorName = "LEVER NOT LOWERED"; 
                GlobalDataManager.instance.AddPoints(-250);
                GlobalDataManager.instance.totalMistakes++;
            }
            FindObjectOfType<ReportSlideController>()?.OpenReport();
        }
    }
}