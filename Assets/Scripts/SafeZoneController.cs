using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform playerTransform; 
    public float detectionRadius = 2.0f; 
    public LayerMask obstacleLayer;

    [Header("Color Materials")]
    public Material greenMaterial;
    public Material yellowMaterial;
    public Material redMaterial;

    [Header("Visualizer")]
    public MeshRenderer zoneRenderer;

    [Header("Robot UI")] // YENİ: Kafadaki scripti buraya bağlayacağız
    public RobotStatusManager statusManager;

    void Update()
    {
        UpdateSafetyLogic();
    }

    void UpdateSafetyLogic()
    {
        // 1. ENGEL KONTROLÜ
        Collider[] obstacles = Physics.OverlapSphere(transform.position, detectionRadius, obstacleLayer);
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

        // 3. RENK UYGULAMA VE YAZI GÜNCELLEME
        if (dangerFound)
        {
            zoneRenderer.material = redMaterial;
            // YENİ: Kırmızı durumunu kafaya bildir
            if(statusManager != null) statusManager.UpdateStatus("Red");
        }
        else if (playerInside)
        {
            zoneRenderer.material = yellowMaterial;
            // YENİ: Sarı durumunu kafaya bildir
            if(statusManager != null) statusManager.UpdateStatus("Yellow");
        }
        else
        {
            zoneRenderer.material = greenMaterial;
            // YENİ: Yeşil durumunu kafaya bildir
            if(statusManager != null) statusManager.UpdateStatus("Green");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}