using UnityEngine;

public class FollowRobot : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform robotTransform; // Robotun ana objesini buraya at
    
    [Header("Position Settings")]
    public float fixedYPosition = 0.02f; // Yere çok yakın olması için (0.01 - 0.05 arası iyidir)

    [Header("Rotation Settings")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f); // Silindiri yatırmak için genelde X:90 gerekir

    void LateUpdate()
    {
        if (robotTransform != null)
        {
            // 1. POZİSYON: Robotun X ve Z'sini al, Y'yi bizim belirlediğimiz yer yüksekliğine sabitle
            transform.position = new Vector3(robotTransform.position.x, fixedYPosition, robotTransform.position.z);
            
            // 2. ROTASYON: Robot ne yaparsa yapsın silindir bizim verdiğimiz açıda kalsın
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }
}
