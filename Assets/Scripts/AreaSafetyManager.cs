using UnityEngine;

public class AreaSafetyManager : MonoBehaviour
{
    public static AreaSafetyManager instance;

    [Header("References")]
    [SerializeField] private SafeZoneController safeZone; 

    void Awake() 
    { 
        if (instance == null) instance = this; 
    }

    public void CheckSafetyAndPunish(string actionName)
    {
        if (safeZone == null || safeZone.zoneRenderer == null) return;

        // Materyal üzerinden hangi bölgede olduğumuzu anlıyoruz
        Material currentMat = safeZone.zoneRenderer.sharedMaterial;

        // 1. KIRMIZI ALAN (DANGER FOUND)
        if (currentMat == safeZone.redMaterial) 
        {
            int penalty = 0;
            switch (actionName)
            {
                case "InstallBattery": penalty = -100; break;
                case "StartRobot":    penalty = -200; break;
                case "Animations":    penalty = -300; break;
                case "Movement":      penalty = -100; break;
            }
            ApplyPenalty(penalty, "DANGER: RED ZONE - " + actionName);
        }
        // 2. SARI ALAN (PLAYER INSIDE)
        else if (currentMat == safeZone.yellowMaterial)
        {
            int penalty = 0;
            switch (actionName)
            {
                case "StartRobot":    penalty = -100; break;
                case "Animations":    penalty = -200; break;
                case "Movement":      penalty = -50; break;
            }
            ApplyPenalty(penalty, "WARNING: YELLOW ZONE - " + actionName);
        }
        // YEŞİL ALAN (currentMat == safeZone.greenMaterial) ise ceza yok
    }

    private void ApplyPenalty(int amount, string errorMsg)
    {
        if (amount == 0) return;
        
        if (GlobalDataManager.instance != null)
        {
            GlobalDataManager.instance.AddPoints(amount); 
            GlobalDataManager.instance.AddMistake(errorMsg);
            Debug.Log($"<color=red>CEZA!</color> {errorMsg} : {amount} Puan");
        }
    }
}