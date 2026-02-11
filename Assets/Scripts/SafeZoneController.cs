using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform playerTransform; 
    public float detectionRadius = 2.0f; 
    // YENİ: Dedektörün merkezini yerden ne kadar yukarı kaldıralım?
    public float detectionHeightOffset = 0.5f; 
    public LayerMask obstacleLayer;

    [Header("Color Materials")]
    public Material greenMaterial;
    public Material yellowMaterial;
    public Material redMaterial;

    [Header("Visualizer")]
    public MeshRenderer zoneRenderer;

    [Header("Robot UI")]
    public RobotStatusManager statusManager;

    void Update()
    {
        UpdateSafetyLogic();
    }

    void UpdateSafetyLogic()
    {
        // 1. ENGEL KONTROLÜ (Merkez noktası artık yukarıda)
        Vector3 detectionCenter = transform.position + Vector3.up * detectionHeightOffset;
        Collider[] obstacles = Physics.OverlapSphere(detectionCenter, detectionRadius, obstacleLayer);
        
        bool dangerFound = false;

        foreach (var col in obstacles)
        {
            // Kendimizi, Player'ı veya zemini algılamamak için ek güvenlik
            if (!col.CompareTag("Player") && col.gameObject != this.gameObject)
            {
                dangerFound = true;
                break;
            }
        }

        // 2. OYUNCU MESAFESİ (Yerdeki yatay mesafe kontrolü aynı kalıyor)
        Vector3 zonePos = transform.position;
        Vector3 playerPos = playerTransform.position;
        zonePos.y = 0;
        playerPos.y = 0;

        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        // 3. RENK UYGULAMA VE YAZI GÜNCELLEME
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
        else
        {
            zoneRenderer.material = greenMaterial;
            if(statusManager != null) statusManager.UpdateStatus("Green");
        }
    }

    // Gizmos kısmını da güncelledim ki Inspector'da neresi taranıyor gör
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 detectionCenter = transform.position + Vector3.up * detectionHeightOffset;
        Gizmos.DrawWireSphere(detectionCenter, detectionRadius);
    }
}