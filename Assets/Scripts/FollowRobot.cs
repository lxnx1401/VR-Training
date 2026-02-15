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
        // Aktif robotu bulalım
        if (offRobot != null && offRobot.gameObject.activeInHierarchy)
            activeTarget = offRobot;
        else if (onRobot != null && onRobot.gameObject.activeInHierarchy)
            activeTarget = onRobot;

        // Takip et
        if (activeTarget != null)
        {
            transform.position = new Vector3(activeTarget.position.x, fixedYPosition, activeTarget.position.z);
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }
}