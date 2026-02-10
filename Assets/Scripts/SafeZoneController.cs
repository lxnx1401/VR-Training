using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform playerTransform; 
    public float detectionRadius = 2.0f; 
    public LayerMask obstacleLayer; // SAKIN "Everything" YAPMA, SADECE "Obstacle" KATMANINI SEÇ

    [Header("Color Materials")]
    public Material greenMaterial;
    public Material yellowMaterial;
    public Material redMaterial;

    [Header("Visualizer")]
    public MeshRenderer zoneRenderer;

    void Update()
    {
        UpdateSafetyLogic();
    }

    void UpdateSafetyLogic()
    {
        // 1. ENGEL KONTROLÜ (OverlapSphere)
        Collider[] obstacles = Physics.OverlapSphere(transform.position, detectionRadius, obstacleLayer);
        bool dangerFound = false;

        foreach (var col in obstacles)
        {
            // Player'ı ve zemini engel saymasın diye kontrol
            if (!col.CompareTag("Player") && col.gameObject != this.gameObject)
            {
                dangerFound = true;
                break;
            }
        }

        // 2. OYUNCU MESAFESİ (Yüksekliği görmezden geliyoruz)
        Vector3 zonePos = transform.position;
        Vector3 playerPos = playerTransform.position;
        
        // Y değerlerini eşitliyoruz ki sadece yerdeki uzaklığa baksın
        zonePos.y = 0;
        playerPos.y = 0;

        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        // 3. RENK UYGULAMA VE DEBUG
        if (dangerFound)
        {
            zoneRenderer.material = redMaterial;
            // Debug.Log("Status: RED (Obstacle Detected)");
        }
        else if (playerInside)
        {
            zoneRenderer.material = yellowMaterial;
            // Debug.Log("Status: YELLOW (Player Inside)");
        }
        else
        {
            zoneRenderer.material = greenMaterial;
            // Debug.Log("Status: GREEN (Clear)");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}