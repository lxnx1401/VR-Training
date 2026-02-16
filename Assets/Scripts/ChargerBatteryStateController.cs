using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChargerBatteryStateController : MonoBehaviour
{
    [Header("Charger Socket (XR)")]
    [SerializeField] private XRSocketInteractor chargerSocket;

    [Header("Battery Animator (INT state)")]
    [SerializeField] private Animator batteryAnimator;

    [Tooltip("INT parameter name in the battery animator (example: BatteryState).")]
    [SerializeField] private string stateIntName = "BatteryState";

    [Header("State Values")]
    [SerializeField] private int stateOff = 0;
    [SerializeField] private int stateChargingLoading = 5; // set this to your 'Charging/Loading' state value

    [Header("Optional safety")]
    [Tooltip("If true, only react when the selected object is actually the battery animator owner.")]
    [SerializeField] private bool requireMatchedBatteryObject = false;

    private bool lastHasSelection;

    private void Awake()
    {
        if (!chargerSocket) chargerSocket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        if (chargerSocket != null)
        {
            chargerSocket.selectEntered.AddListener(OnSocketSelectEntered);
            chargerSocket.selectExited.AddListener(OnSocketSelectExited);
        }

        // Initialize state based on current socket status
        lastHasSelection = (chargerSocket != null && chargerSocket.hasSelection);
        if (lastHasSelection)
            SetBatteryState(stateChargingLoading);
        else
            SetBatteryState(stateOff);
    }

    private void OnDisable()
    {
        if (chargerSocket != null)
        {
            chargerSocket.selectEntered.RemoveListener(OnSocketSelectEntered);
            chargerSocket.selectExited.RemoveListener(OnSocketSelectExited);
        }
    }

    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        if (!IsValidBattery(args.interactableObject)) return;
        SetBatteryState(stateChargingLoading);
    }

    private void OnSocketSelectExited(SelectExitEventArgs args)
    {
        if (!IsValidBattery(args.interactableObject)) return;
        SetBatteryState(stateOff);
    }

    private bool IsValidBattery(IXRSelectInteractable interactable)
    {
        if (!requireMatchedBatteryObject) return true;

        if (batteryAnimator == null) return false;
        if (interactable == null) return false;

        var mono = interactable.transform;
        if (mono == null) return false;

        // If the animator is on the battery root, require the socket object to be that root (or a child)
        return mono == batteryAnimator.transform || mono.IsChildOf(batteryAnimator.transform) || batteryAnimator.transform.IsChildOf(mono);
    }

    private void SetBatteryState(int state)
    {
        if (batteryAnimator == null) return;
        if (string.IsNullOrWhiteSpace(stateIntName)) return;

        batteryAnimator.SetInteger(stateIntName, state);
    }
}
