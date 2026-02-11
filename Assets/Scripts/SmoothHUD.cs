using UnityEngine;

public class SmoothHUD : MonoBehaviour
{
    public Transform cameraTransform; // Buraya Main Camera'yı sürükle
    public float distance = 1.5f;     // Gözünden ne kadar uzakta dursun?
    public float followSpeed = 5f;    // Takip hızı (ne kadar yüksekse o kadar sert takip eder)

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. Hedef Pozisyon: Kameranın tam önünde, belirlediğimiz mesafede bir nokta
        Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * distance);
        
        // 2. Yumuşak Takip (Lerp): Mevcut pozisyondan hedef pozisyona yumuşak geçiş
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 3. Bakış: Panel her zaman oyuncuya baksın
        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
    }
}