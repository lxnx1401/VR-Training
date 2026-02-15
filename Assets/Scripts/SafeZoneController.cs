using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform playerTransform; 
    public float detectionRadius = 2.0f; 
    public float detectionHeightOffset = 0.5f; 
    public LayerMask obstacleLayer;

    [Header("Robot References")]
    public Transform offRobot;
    public Transform onRobot;

    [Header("Color Materials")]
    public Material greenMaterial;
    public Material yellowMaterial;
    public Material redMaterial;

    [Header("Visualizer")]
    public MeshRenderer zoneRenderer;

    [Header("Robot UI & Audio")]
    // --- AGA BURASI ÖNEMLİ: Warning panellerini buraya sürükle ---
    public RobotStatusManager offStatusManager; 
    public RobotStatusManager onStatusManager;
    public RobotSpeechManager speechManager; 

    private bool task3Completed = false;
    private bool task4Completed = false;
    private bool challengePositionTaskDone = false;
    private float activationDelay = 3.0f; 
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        UpdateSafetyLogic();
    }

    // Aktif olan Status Manager'ı bulup ona mesaj gönderen yardımcı fonksiyon
    private void SendStatusMessage(string message)
    {
        if (offRobot != null && offRobot.gameObject.activeInHierarchy && offStatusManager != null)
            offStatusManager.UpdateStatus(message);
        else if (onRobot != null && onRobot.gameObject.activeInHierarchy && onStatusManager != null)
            onStatusManager.UpdateStatus(message);
    }

    void UpdateSafetyLogic()
    {
        Vector3 detectionCenter = transform.position + Vector3.up * detectionHeightOffset;
        Collider[] obstacles = Physics.OverlapSphere(detectionCenter, detectionRadius, obstacleLayer);
        
        bool dangerFound = false;
        foreach (var col in obstacles)
        {
            if (col.isTrigger) continue;

            bool isPlayer = col.CompareTag("Player");
            bool isRobotPart = (offRobot != null && col.transform.IsChildOf(offRobot)) || 
                               (onRobot != null && col.transform.IsChildOf(onRobot));

            if (!isPlayer && !isRobotPart && col.gameObject != this.gameObject)
            {
                dangerFound = true;
                break;
            }
        }

        Vector3 zonePos = transform.position;
        Vector3 playerPos = playerTransform.position;
        zonePos.y = 0; playerPos.y = 0;
        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        if (dangerFound)
        {
            zoneRenderer.material = redMaterial;
            SendStatusMessage("Red"); // Aktif robota mesajı gönderir
        }
        else if (playerInside) 
        {
            zoneRenderer.material = yellowMaterial;
            SendStatusMessage("Yellow");

            if (speechManager != null && speechManager.GetCurrentIndex() == 3 && 
                speechManager.isWaitingForTask && !task3Completed)
            {
                task3Completed = true; 
                speechManager.isWaitingForTask = false; 
                speechManager.PlayNextLine(); 
            }
        }
        else 
        {
            zoneRenderer.material = greenMaterial;
            SendStatusMessage("Green");

            if (speechManager != null && speechManager.GetCurrentIndex() == 5 && 
                speechManager.isWaitingForTask && !task4Completed)
            {
                task4Completed = true; 
                speechManager.isWaitingForTask = false; 
                speechManager.PlayNextLine(); 
            }

            if (timer > activationDelay && !challengePositionTaskDone)
            {
                challengePositionTaskDone = true;
                if (TaskUIManager.instance != null)
                {
                    TaskUIManager.instance.CompleteTask("Position");
                }
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