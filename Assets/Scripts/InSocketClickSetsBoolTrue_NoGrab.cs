using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InSocketClickSetsBoolTrue_NoGrab : MonoBehaviour
{
    [Header("Socket state (Script liegt am Socket)")]
    [SerializeField] private BatterySocketState socketState;

    [Header("Input fuer PC-Test / XR Action")]
    [SerializeField] private InputActionReference clickAction;

    [Header("Animator Target")]
    [SerializeField] private Animator targetAnimator;

    [Header("Animator INT param")]
    [SerializeField] private string stateIntName = "BatteryState";

    [Header("State Values")]
    [SerializeField] private int stateOff = 0;
    [SerializeField] private int statePowerOn = 1;     // plays PowerOn animation then goes to OnIdle
    [SerializeField] private int stateOnIdle = 2;      // stable ON
    [SerializeField] private int stateUsedIdle = 3;    // stable USED
    [SerializeField] private int statePowerOff = 4;    // plays PowerOff animation then goes to Off

    [Header("Spam protection")]
    [SerializeField] private float cooldownSeconds = 3.0f;
    private float lastToggleTime = -999f;

    [Header("Optional: tell Module06 when power off started")]
    [SerializeField] private Module06Manager module06;

    [Header("Grab Interactable")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private bool batteryTaskCompleted = false;

    private void Awake()
    {
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grab != null)
            grab.selectEntered.AddListener(OnGrabSelectEntered);

        if (clickAction != null)
        {
            clickAction.action.Enable();
            clickAction.action.performed += OnClick;
        }
    }

    private void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(OnGrabSelectEntered);

        if (clickAction != null)
            clickAction.action.performed -= OnClick;
    }

    private void OnGrabSelectEntered(SelectEnterEventArgs args)
    {
        // Task trigger when a HAND grabs it (not socket)
        if (!(args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))
        {
            if (!batteryTaskCompleted)
            {
                batteryTaskCompleted = true;
                if (TaskUIManager.instance != null)
                    TaskUIManager.instance.CompleteTask("LocateBattery");
            }
        }

        // NoGrab while in socket
        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        // allow socket to select it
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            return;

        // force release if a hand tries to grab while it's in socket
        if (grab != null && grab.interactionManager != null)
            grab.interactionManager.SelectExit(args.interactorObject, grab);
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        // 1. Soket kontrolü
        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        // --- YENİ KISIM: IŞIN KONTROLÜ (ÇAKIŞMAYI ÖNLEYEN FİLTRE) ---
        // Sadece ışınımız (Raycast) bu bataryaya çarpıyorsa işlemi devam ettir.
        // Bu sayede Start butonuna basarken batarya kendi kendine tetiklenmez.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
      if (Physics.Raycast(ray, out RaycastHit hit, 5.0f))
{
    string hitName = hit.transform.name;
    Debug.Log("<color=cyan>Işın Şuna Takıldı: </color>" + hitName);

    // 1. KONTROL: Direkt batarya mı?
    bool hitMe = (hit.transform == this.transform || hit.transform.IsChildOf(this.transform));

    // 2. KONTROL: Robot gövdesi mi? 
    // Logda "RobotOff" gördüğün için sadece "Off" kelimesini aratmak daha garantidir
    bool hitRobot = hitName.ToLower().Contains("robot") || hitName.ToLower().Contains("off");

    if (hitMe || hitRobot)
    {
        Debug.Log("<color=green>HEDEF DOĞRU: İşlem başlatılıyor...</color>");
        // Buradan aşağısı devam edecek, return yapmıyoruz!
    }
    else
    {
        Debug.Log("Işın alakasız bir yere (" + hitName + ") çarptı, iptal.");
        return;
    }
}
        // ----------------------------------------------------------

        if (Time.time - lastToggleTime < cooldownSeconds)
            return;

        if (targetAnimator == null || string.IsNullOrEmpty(stateIntName))
            return;

        lastToggleTime = Time.time;

        int current = targetAnimator.GetInteger(stateIntName);

        bool isOff = (current == stateOff);
        bool isOnOrUsed = (current == stateOnIdle || current == stateUsedIdle);

        if (isOff)
        {
            targetAnimator.SetInteger(stateIntName, statePowerOn);
            if (RobotStartupManager.instance != null)
                RobotStartupManager.instance.hasClickedBattery = true;
        }
        else if (isOnOrUsed)
        {
            targetAnimator.SetInteger(stateIntName, statePowerOff);
            if (RobotStartupManager.instance != null)
                RobotStartupManager.instance.hasClickedBattery = false; 

            if (module06 != null)
                module06.NotifyBatteryPowerOff_Explicit();
        }
    }
    // Bu metod Unity Event'lerinden (Activated gibi) çağrılabilecek
public void TriggerClickManual(SelectEnterEventArgs args)
{
    // 1. KONTROL: Eğer etkileşime giren şey bir SOKET ise, bu bir tıklama değildir!
    // Sadece el/ışın (Interactor) etkileşime girdiğinde çalış
    if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
    {
        return; // Soket takıldı, hiçbir şey yapma ve çık.
    }

    // 2. KONTROL: Eğer batarya zaten sokette değilse yine tıklanmasın
    if (socketState == null || !socketState.IsBatteryInSocket) return;

    Debug.Log("EL/IŞIN İLE TIKLANDI! Robot kilitleri açılıyor...");
    
    // Asıl tıklama mantığını çalıştır
    OnClick(default); 
}

    // Optional helper for other scripts: set USED state explicitly
    public void SetUsedState(bool used)
    {
        if (targetAnimator == null || string.IsNullOrEmpty(stateIntName)) return;

        int current = targetAnimator.GetInteger(stateIntName);
        if (current == stateOnIdle || current == stateUsedIdle)
            targetAnimator.SetInteger(stateIntName, used ? stateUsedIdle : stateOnIdle);
    }

    // Optional: call from animation event at end of PowerOff clip
    public void ForceOffState()
    {
        if (targetAnimator == null || string.IsNullOrEmpty(stateIntName)) return;
        targetAnimator.SetInteger(stateIntName, stateOff);
    }
}
