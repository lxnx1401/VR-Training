using UnityEngine;
using TMPro;

public class RobotStatusManager : MonoBehaviour
{
    private Transform mainCameraTransform;
    
    [Header("Referanslar")]
    public TextMeshProUGUI statusText; // Kafadaki TMP'yi buraya sürükle
    // Buraya senin silindir scriptini bağlayacağız. 
    // Örn: public CylinderScript cylinder; 

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 1. Her zaman oyuncuya bakma (Billboard efekti)
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                         mainCameraTransform.rotation * Vector3.up);
    }

    // Bu fonksiyonu senin silindir scriptin renk değiştirdikçe çağıracağız
    public void UpdateStatus(string state)
    {
        switch (state)
        {
            case "Red":
                statusText.text = "DANGER: OBSTACLE!";
                statusText.color = Color.red;
                break;
            case "Yellow":
                statusText.text = "WARNING: CLEAR AREA";
                statusText.color = Color.yellow;
                break;
            case "Green":
                statusText.text = "POSITION SAFE";
                statusText.color = Color.green;
                break;
        }
    }
}