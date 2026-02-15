using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InSocketClickSetsBoolTrue_NoGrab : MonoBehaviour
{
    [Header("Socket state (Script liegt am Socket)")]
    [SerializeField] private BatterySocketState socketState;

    [Header("Input für PC-Test (Left Mouse)")]
    [SerializeField] private InputActionReference clickAction; 

    [Header("Animator Target")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string boolName = "Pressed";

    [Header("Grab Interactable")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    // --- CHALLENGE MODU EKLEMESİ ---
    private bool batteryTaskCompleted = false;
    // -------------------------------

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
        // --- AGA BURASI GÖREVİ TETİKLEDİĞİMİZ YER ---
        // Eğer tutan şey bir Socket değilse (yani bir el/interactor ise) ve görev henüz bitmediyse
        if (!(args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))
        {
            if (!batteryTaskCompleted)
            {
                batteryTaskCompleted = true;
                if (TaskUIManager.instance != null)
                {
                    TaskUIManager.instance.CompleteTask("LocateBattery");
                }
            }
        }
        // --------------------------------------------

        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            return;

        if (grab.interactionManager != null)
            grab.interactionManager.SelectExit(args.interactorObject, grab);
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        if (targetAnimator == null || string.IsNullOrEmpty(boolName))
            return;

        targetAnimator.SetBool(boolName, true);
    }
}
