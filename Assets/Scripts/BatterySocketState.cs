using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BatterySocketState : MonoBehaviour
{


    
    
    public bool IsBatteryInSocket { get; private set; }

    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    [Header("Robot Swap Settings")]
    [Tooltip("Sahnede şu an duran, çalışan/aktif olmayan robot")]
    public GameObject offRobot; 

    [Header("Shutdown Fix")]
public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable robotMainGrab;
    
    [Tooltip("Kumanda ile çalışabilen, aktif robot (Başta kapalı olmalı)")]
    public GameObject onRobot;  

    [Tooltip("Değişim anında çıkacak efekt (Opsiyonel)")]
    public GameObject swapEffect; 

    private bool installTaskDone = false;

    private void Awake()
    {
        // Soketi otomatik bul veya atanmadıysa uyar
        if (!socket) socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        
        // Oyun başında çalışan robot kapalı olmalı
        if(onRobot != null) onRobot.SetActive(false);
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnEntered);
        socket.selectExited.AddListener(OnExited);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnEntered);
        socket.selectExited.RemoveListener(OnExited);
    }
private void OnEntered(SelectEnterEventArgs args)
{ 
    if (GlobalDataManager.instance != null)
{
    GlobalDataManager.instance.isBatteryInstalled = true;
}
    IsBatteryInSocket = true;

    // 1. GÖREV KONTROLÜ (Tag'i "Battery" olan obje girdiyse)
    if (args.interactableObject.transform.CompareTag("Battery") && !installTaskDone)
    {
        // ---------------------------------------------------------
        // CEZA SİSTEMİ BURADA: Puan vermeden önce alanı kontrol et!
        if (AreaSafetyManager.instance != null)
        {
            AreaSafetyManager.instance.CheckSafetyAndPunish("InstallBattery");
        }
        // ---------------------------------------------------------

        installTaskDone = true;
        
        // Panodaki görevi tetikle
        if (TaskUIManager.instance != null)
        {
            TaskUIManager.instance.CompleteTask("InstallBattery");
        }

        // Puan ver
        if (GlobalDataManager.instance != null)
        {
            GlobalDataManager.instance.AddPoints(150);
        }
        if (RobotStartupManager.instance != null)
        {
            RobotStartupManager.instance.hasInsertedBattery = true;
        }

        
    }
}

    public void SwapRobots()
    {
        if (offRobot != null && onRobot != null)
        {
            // Rigidbody sıfırlama (Titremeyi önlemek için)
            Rigidbody rb = onRobot.GetComponent<Rigidbody>(); // onRobot'u kontrol etmeliyiz çünkü o yeni doğuyor
            if(rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 1. Bataryayı yok et
            if (socket.interactablesSelected.Count > 0)
            {
                GameObject battery = socket.interactablesSelected[0].transform.gameObject;
                Destroy(battery); 
            }

            // 2. Pozisyonu ayarla (Y eksenini düzeltiyoruz)
            Vector3 spawnPos = offRobot.transform.position;
            spawnPos.y -= 1.0f; // On robot, Off robottan 1 metre yukarıda doğsun
            
            onRobot.transform.position = spawnPos;
            onRobot.transform.rotation = offRobot.transform.rotation;

            onRobot.transform.rotation = offRobot.transform.rotation * Quaternion.Euler(0, 180f, 0);

            // 3. Robotları değiştir
            offRobot.SetActive(false);
            onRobot.SetActive(true);
            

            if (swapEffect != null) Instantiate(swapEffect, onRobot.transform.position, Quaternion.identity);
        }
    }
public void SwapToOff()
    {
        if (onRobot != null && offRobot != null && onRobot.activeSelf)
        {
            // Rigidbody sıfırlama
            Rigidbody rb = offRobot.GetComponent<Rigidbody>();
            if(rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 1. Pozisyonu al ve 1 metre aşağı indir (Havada asılı kalmasın diye)
            Vector3 targetPos = onRobot.transform.position;
            targetPos.y += 1.0f; // YUKARIDAKİ robotu AŞAĞI indiriyoruz
            
            offRobot.transform.position = targetPos;
            
            // 2. 180 derece dönüşü koru
            offRobot.transform.rotation = onRobot.transform.rotation * Quaternion.Euler(0, 180f, 0);

            // 3. Değişimi yap
            onRobot.SetActive(false);
            offRobot.SetActive(true);

            if (robotMainGrab != null) 
        {
            robotMainGrab.enabled = false; 
            Debug.Log("Ana gövde tutma kapatıldı.");
        }

            
            if (swapEffect != null) Instantiate(swapEffect, offRobot.transform.position, Quaternion.identity);
        }
        
    }

    private void OnExited(SelectExitEventArgs args)
    {
        IsBatteryInSocket = false;
    }
}