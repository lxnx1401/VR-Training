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
    // YENİ: Konuşma sistemine erişim
    public RobotSpeechManager speechManager; 

    private bool task3Completed = false;

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

        // 2. OYUNCU MESAFESİ
        Vector3 zonePos = transform.position;
        Vector3 playerPos = playerTransform.position;
        zonePos.y = 0;
        playerPos.y = 0;

        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        // 3. RENK UYGULAMA VE GÖREV KONTROLÜ
        if (dangerFound)
        {
            zoneRenderer.material = redMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Red");
        }
        else if (playerInside)
        {
            zoneRenderer.material = yellowMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Yellow");
        }
        else // BURASI YEŞİL DURUMU
        {
            zoneRenderer.material = greenMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Green");

            // --- ETKİLEŞİMLİ EĞİTİM MANTIĞI ---
            // Eğer 3. cümledeysek (Robotu Taşı Görevi) ve alan yeşilse:
            if (speechManager != null && speechManager.GetCurrentIndex() == 2 && !task3Completed)
            {
                // Robotun "Waiting" modunu kapat ve bir sonraki cümleye (Great!) geç
                task3Completed = true; // Bu görevin bir kez tetiklenmesi için
                speechManager.isWaitingForTask = false; 
                speechManager.PlayNextLine(); 
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