using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BatterySocketState : MonoBehaviour
{
    public bool IsBatteryInSocket { get; private set; }

    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    [Header("Robot Swap Settings")]
    [Tooltip("Sahnede şu an duran, çalışan/aktif olmayan robot")]
    public GameObject offRobot; 
    
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
        IsBatteryInSocket = true;

        // 1. GÖREV KONTROLÜ (Tag'i "Battery" olan obje girdiyse)
        if (args.interactableObject.transform.CompareTag("Battery") && !installTaskDone)
        {
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

            // 2. ROBOT DEĞİŞTİRME (SWAP)
            SwapRobots();
        }
    }

    private void SwapRobots()
{
    if (offRobot != null && onRobot != null)
    {
        // 1. Bataryayı yok et (Görsel karmaşa bitsin)
        if (socket.interactablesSelected.Count > 0)
        {
            GameObject battery = socket.interactablesSelected[0].transform.gameObject;
            Destroy(battery); 
        }

        // 2. Pozisyonu ve rotasyonu birebir kopyala
        onRobot.transform.position = offRobot.transform.position;
        onRobot.transform.rotation = offRobot.transform.rotation;

        // 3. KRİTİK: Robotları değiştir
        offRobot.SetActive(false); // Eskisi tamamen kapansın
        onRobot.SetActive(true);   // Yenisi tam o noktada belirsin

        if (swapEffect != null) Instantiate(swapEffect, onRobot.transform.position, Quaternion.identity);
    }
}

    private void OnExited(SelectExitEventArgs args)
    {
        IsBatteryInSocket = false;
    }
}