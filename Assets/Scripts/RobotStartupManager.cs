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
    // Kol YUKARI çekildiyse
    if (currentState == true) 
    {
        hasLiftedLever = true;
        hasLoweredLever = false; // Tekrar kaldırıldıysa indirilmiş sayılmaz
        Debug.Log("Kol yukarıda.");
    }
    // Kol AŞAĞI indirildiyse (hasClickedBattery şartını sildik!)
    else 
    {
        hasLoweredLever = true;
        Debug.Log("Kol aşağıda.");
    }
}

    // --- START TUŞU FONKSİYONU ---
    // Bu metodu Start butonunun OnClick event'ine bağlayacaksın
   public void OnStartButtonPressed()
{
    // 1. KİLİT: Batarya fiziksel olarak sokette mi?
    if (socketState == null || !socketState.IsBatteryInSocket)
    {
        Debug.LogWarning("Batarya yok! Robot çalıştırılamaz.");
        return; 
    }

    // 2. KİLİT: Bataryaya tıklanıp "ON" konumuna getirildi mi?
    // hasClickedBattery değişkenini InSocketClick scriptinden dolduruyorduk
    if (!hasClickedBattery)
    {
        Debug.LogWarning("Batarya takılı ama aktif değil (Tıklanmadı)!");
        // Oyuncuya burada "Bataryayı aktif etmelisin" gibi bir uyarı verebilirsin
        return;
    }

    // --- BURADAN AŞAĞISI SADECE İKİ KİLİT DE AÇILDIYSA ÇALIŞIR ---

    if (socketState.offRobot.activeSelf)
    {
        socketState.SwapRobots();
        
        if (TaskUIManager.instance != null)
        {
            TaskUIManager.instance.CompleteTask("StartRobot");
        }
    }

    CheckProcedureAndScore();
}

  private void CheckProcedureAndScore()
{
    // Tek kural: Kol aşağıda mı?
    if (hasLoweredLever)
    {
        // BAŞARI: Puanı ver ama lastErrorName'e dokunma (Eski hata kalsın)
        Debug.Log("<color=green>Başarılı çalıştırma!</color>");
        
    }
    else
    {
        // HATA: Kolu unuttun. Deftere yaz, puanı kes.
        Debug.Log("<color=red>Hata: Kol indirilmedi!</color>");
        
        if (GlobalDataManager.instance != null)
        {
            GlobalDataManager.instance.lastErrorName = "LEVER NOT LOWERED"; 
            GlobalDataManager.instance.AddPoints(-250);
            GlobalDataManager.instance.totalMistakes++;
        }

        // Rapor panosunu aç ki hatayı görsün
        FindObjectOfType<ReportSlideController>()?.OpenReport();
    }
}
}