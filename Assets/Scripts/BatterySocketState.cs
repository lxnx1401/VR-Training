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

   

    private void OnExited(SelectExitEventArgs args)
    {
        IsBatteryInSocket = false;

        if (RobotShutdownManager.instance != null)
    {
        RobotShutdownManager.instance.OnBatteryExtracted();
    }
    }
}