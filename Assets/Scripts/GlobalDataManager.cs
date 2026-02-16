using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager instance;
    public bool isBatteryInstalled = false;

    [Header("Session Data")]
    public int currentScore = 0;
    public int totalMistakes = 0;
    public string lastErrorName = "NONE DETECTED"; // Son hatayı tutar
    public float sessionTimer = 0f;
    public bool isTimerActive = false;

    [Header("Playstyle Tracking")]
    public int totalInputs = 0; // Oyuncu ne kadar "panik" yapıyor?

    private string HIGH_SCORE_KEY = "GlobalHighScore";

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (isTimerActive) sessionTimer += Time.deltaTime;
        
        // Basit bir input takibi (Herhangi bir tuşa basıldı mı?)
        if (Input.anyKeyDown) totalInputs++;
    }

    // Artık hata eklerken ismini de giriyoruz
    public void AddMistake(string errorDescription) 
    {
        totalMistakes++;
        lastErrorName = errorDescription.ToUpper(); 
    }

    public void AddPoints(int amount) => currentScore += amount;

    public void ResetSession()
    {
        currentScore = 0;
        totalMistakes = 0;
        sessionTimer = 0f;
        lastErrorName = "NONE DETECTED";
        totalInputs = 0;
    }
}
