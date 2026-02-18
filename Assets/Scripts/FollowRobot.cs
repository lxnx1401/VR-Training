using UnityEngine;

public class FollowRobot : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform offRobot; 
    public Transform onRobot;
    
    private Transform activeTarget; // Dinamik olarak belirlenecek

    [Header("Position Settings")]
    public float fixedYPosition = 0.02f; 

    [Header("Rotation Settings")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

   void LateUpdate()
    {
        // Önce OnRobot'u kontrol et (Çünkü o önceliklidir, aktifse odur)
        if (onRobot != null && onRobot.gameObject.activeInHierarchy)
        {
            activeTarget = onRobot;
        }
        // Değilse OffRobot'u kontrol et
        else if (offRobot != null && offRobot.gameObject.activeInHierarchy)
        {
            activeTarget = offRobot;
        }

        // Takip et
        if (activeTarget != null)
        {
            // Robotun sadece altındaki zemini takip etmesi için:
            transform.position = new Vector3(activeTarget.position.x, fixedYPosition, activeTarget.position.z);
            
            // Robotun rotasyonu değiştikçe silindirin rotasyonu bozulmasın diye fixed rotation:
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }
}