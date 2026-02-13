using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform playerTransform; 
    public float detectionRadius = 2.0f; 
    public float detectionHeightOffset = 0.5f; 
    public LayerMask obstacleLayer;

    [Header("Color Materials")]
    public Material greenMaterial;
    public Material yellowMaterial;
    public Material redMaterial;

    [Header("Visualizer")]
    public MeshRenderer zoneRenderer;

    [Header("Robot UI & Audio")]
    public RobotStatusManager statusManager;
    public RobotSpeechManager speechManager; 

    // Görevlerin sadece birer kez tetiklenmesi için bayraklar
    private bool task3Completed = false;
    private bool task4Completed = false;

    void Update()
    {
        UpdateSafetyLogic();
    }

    void UpdateSafetyLogic()
    {
        // 1. ENGEL KONTROLÜ
        Vector3 detectionCenter = transform.position + Vector3.up * detectionHeightOffset;
        Collider[] obstacles = Physics.OverlapSphere(detectionCenter, detectionRadius, obstacleLayer);
        
        bool dangerFound = false;
        foreach (var col in obstacles)
        {
            if (!col.CompareTag("Player") && col.gameObject != this.gameObject)
            {
                dangerFound = true;
                break;
            }
        }

        // 2. OYUNCU MESAFESİ (Yerdeki yatay mesafe)
        Vector3 zonePos = transform.position;
        Vector3 playerPos = playerTransform.position;
        zonePos.y = 0; playerPos.y = 0;
        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        // 3. RENK UYGULAMA VE ARKADAŞININ İSTEDİĞİ GÜVENLİK MANTIĞI
        if (dangerFound)
        {
            // KIRMIZI: Alanda yabancı madde var (Engel)!
            zoneRenderer.material = redMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Red");
        }
        else if (playerInside) 
        {
            // SARI: Engel yok ama OYUNCU robotun dibinde (Uzaklaşması lazım).
            zoneRenderer.material = yellowMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Yellow");

            // GÖREV 3 TAMAMLANIR: Robotu güvenli yere bıraktın ama hala dibindesin.
            // (3. Cümle: Index 2 - "Robotu güvenli yere koy")
            if (speechManager != null && speechManager.GetCurrentIndex() == 3 && 
                speechManager.isWaitingForTask && !task3Completed)
            {
                task3Completed = true; 
                speechManager.isWaitingForTask = false; 
                speechManager.PlayNextLine(); // Robot: "Şimdi SafeSpot'a git" der.
            }
        }
        else 
        {
            // YEŞİL: Engel yok VE oyuncu güvenli mesafede (Dışarı çıktı).
            zoneRenderer.material = greenMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Green");

            // GÖREV 4 TAMAMLANIR: Oyuncu güvenli mesafeye çekildi.
            // (4. Cümle: Index 3 - "SafeSpot'a git ve aktif et")
            if (speechManager != null && speechManager.GetCurrentIndex() == 5 && 
                speechManager.isWaitingForTask && !task4Completed)
            {
                task4Completed = true; 
                speechManager.isWaitingForTask = false; 
                speechManager.PlayNextLine(); // Robot: "Eğitim bitti, sistem aktif!"
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 detectionCenter = transform.position + Vector3.up * detectionHeightOffset;
        Gizmos.DrawWireSphere(detectionCenter, detectionRadius);
    }
}