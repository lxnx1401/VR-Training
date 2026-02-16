using UnityEngine;

// XRI namespaces
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Module06Manager : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Controller Canvas Root (opened/closed via X)")]
    [SerializeField] private GameObject controllerCanvasRoot;

    [Header("Robot Swap (On -> Off after closing controller)")]
    [SerializeField] private Transform robotOn;
    [SerializeField] private Transform robotOff;
    [SerializeField] private bool toggleActiveOnSwap = true;

    [Header("Battery + RobotOff Socket (XR)")]
    [SerializeField] private XRGrabInteractable batteryGrab;
    [SerializeField] private XRSocketInteractor robotOffBatterySocket;

    [Tooltip("Snap battery into the OFF robot socket right after closing controller.")]
    [SerializeField] private bool snapBatteryIntoOffSocketOnSwap = true;

    [Header("Module 6 Tasks - XR References")]
    [SerializeField] private XRSimpleInteractable leftShoulderInteractable;

    [Header("Battery State (INT)")]
    [SerializeField] private Animator batteryAnimator;
    [SerializeField] private string stateIntName = "BatteryState";

    [Header("State Values")]
    [SerializeField] private int stateOff = 0;
    [SerializeField] private int stateOnIdle = 2;
    [SerializeField] private int stateUsedIdle = 3;
    [SerializeField] private int statePowerOff = 4;

    [Tooltip("Force battery state to ON after robot swap + snap, so you can turn it OFF without first clicking.")]
    [SerializeField] private bool forceBatteryOnAfterSwap = true;

    [Tooltip("Which ON state should be forced after swap? Usually OnIdle (2) or UsedIdle (3).")]
    [SerializeField] private int forceOnStateValue = 2;

    [Header("Anti-unwanted state jumps (debug safety)")]
    [Tooltip("If battery jumps to PowerOff (4) before we are on the PowerOff task line, we auto-correct back to ON.")]
    [SerializeField] private bool autoCorrectEarlyPowerOff = true;

    [Tooltip("Print warnings when a wrong state jump is detected.")]
    [SerializeField] private bool logStateIssues = true;

    [Tooltip("Socket on the charger where the battery must be placed.")]
    [SerializeField] private XRSocketInteractor chargerSocket;

    [Header("Task line indices (0-based)")]
    [SerializeField] private int idxPressShutdown = 3;
    [SerializeField] private int idxCloseController = 4;
    [SerializeField] private int idxLiftArm = 5;
    [SerializeField] private int idxBatteryPowerOff = 6;
    [SerializeField] private int idxRemoveBattery = 7;
    [SerializeField] private int idxPlaceOnCharger = 8;

    // --- task flags ---
    private bool shutdownPressed;
    private bool armIsUp;
    private bool batteryPowerOffDone;

    private bool swappedToOffRobot;
    private bool forcedStateOnce;
    private int lastIdx = -999;

    private void OnEnable()
    {
        if (leftShoulderInteractable != null)
            leftShoulderInteractable.selectEntered.AddListener(OnShoulderSelected);
    }

    private void OnDisable()
    {
        if (leftShoulderInteractable != null)
            leftShoulderInteractable.selectEntered.RemoveListener(OnShoulderSelected);
    }

    private void Update()
    {
        if (speechManager == null) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx != lastIdx)
        {
            OnLineChanged(idx);
            lastIdx = idx;
        }

        // Safety: battery should NOT enter PowerOff early
        if (autoCorrectEarlyPowerOff)
            GuardAgainstEarlyPowerOff(idx);

        if (!speechManager.isWaitingForTask) return;

        // Task: Press Shutdown
        if (idx == idxPressShutdown)
        {
            if (shutdownPressed) Advance();
            return;
        }

        // Task: Close controller -> swap robot + snap + (force battery ON once)
        if (idx == idxCloseController)
        {
            if (controllerCanvasRoot != null && !controllerCanvasRoot.activeInHierarchy)
            {
                if (!swappedToOffRobot)
                {
                    SwapRobotOnToOff_CopyXZOnly();
                    swappedToOffRobot = true;

                    if (snapBatteryIntoOffSocketOnSwap)
                        SnapBatteryIntoOffSocket();
                }

                // Force battery ON only ONCE and only after swap
                if (forceBatteryOnAfterSwap && swappedToOffRobot && !forcedStateOnce)
                {
                    ForceBatteryStateOn();
                    forcedStateOnce = true;
                }

                Advance();
            }
            return;
        }

        // Task: Lift arm
        if (idx == idxLiftArm)
        {
            if (armIsUp)
                Advance();
            return;
        }

        // Task: Power OFF
        if (idx == idxBatteryPowerOff)
        {
            if (batteryPowerOffDone)
            {
                Advance();
                return;
            }

            // Completed when state is OFF
            if (IsBatteryInOffSocket() && IsBatteryStateOff())
            {
                batteryPowerOffDone = true;
                Advance();
            }
            return;
        }

        // Task: Remove battery
        if (idx == idxRemoveBattery)
        {
            if (!batteryPowerOffDone) return;

            if (IsBatteryHeldByHand())
                Advance();

            return;
        }

        // Task: Place on charger
        if (idx == idxPlaceOnCharger)
        {
            if (chargerSocket != null && chargerSocket.hasSelection)
                Advance();

            return;
        }
    }

    private void OnLineChanged(int newIdx)
    {
        if (newIdx == idxLiftArm) armIsUp = false;
        if (newIdx == idxBatteryPowerOff) batteryPowerOffDone = false;

        if (newIdx == idxRemoveBattery)
            UnsocketBatteryFromOffSocket();
    }

    private void Advance()
    {
        speechManager.UnlockTask();
        speechManager.PlayNextLine();
    }

    private void SwapRobotOnToOff_CopyXZOnly()
    {
        if (robotOn == null || robotOff == null) return;

        Vector3 offPos = robotOff.position;
        Vector3 onPos = robotOn.position;

        offPos.x = onPos.x;
        offPos.z = onPos.z;

        robotOff.position = offPos;

        if (toggleActiveOnSwap)
        {
            robotOff.gameObject.SetActive(true);
            robotOn.gameObject.SetActive(false);
        }
    }

    // -----------------------
    // Arm logic
    // -----------------------

    private void OnShoulderSelected(SelectEnterEventArgs args)
    {
        armIsUp = true;
        TryAdvanceNow(idxLiftArm);
    }

    public void SetArmUp()
    {
        armIsUp = true;
        TryAdvanceNow(idxLiftArm);
    }

    // -----------------------
    // Battery state helpers
    // -----------------------

    private void ForceBatteryStateOn()
    {
        if (batteryAnimator == null) return;
        if (string.IsNullOrWhiteSpace(stateIntName)) return;

        // Only allow forcing to ON states
        int target = (forceOnStateValue == stateUsedIdle) ? stateUsedIdle : stateOnIdle;
        batteryAnimator.SetInteger(stateIntName, target);
    }

    private void GuardAgainstEarlyPowerOff(int currentSpeechIdx)
    {
        if (batteryAnimator == null) return;
        if (string.IsNullOrWhiteSpace(stateIntName)) return;

        // Before we are in the "power off" task line, we do not accept statePowerOff
        if (currentSpeechIdx < idxBatteryPowerOff)
        {
            int s = batteryAnimator.GetInteger(stateIntName);
            if (s == statePowerOff)
            {
                // Auto-correct to ON idle
                int back = (forceOnStateValue == stateUsedIdle) ? stateUsedIdle : stateOnIdle;
                batteryAnimator.SetInteger(stateIntName, back);

                if (logStateIssues)
                    Debug.LogWarning("[Module06Manager] BatteryState jumped to PowerOff early. Auto-corrected back to ON. Check Animator transitions or other scripts setting BatteryState.");
            }
        }
    }

    private bool IsBatteryStateOff()
    {
        if (batteryAnimator == null) return false;
        if (string.IsNullOrWhiteSpace(stateIntName)) return false;

        int s = batteryAnimator.GetInteger(stateIntName);
        return s == stateOff;
    }

    private bool IsBatteryInOffSocket()
    {
        if (robotOffBatterySocket == null) return true;
        return robotOffBatterySocket.hasSelection;
    }

    private bool IsBatteryHeldByHand()
    {
        if (batteryGrab == null) return false;
        if (!batteryGrab.isSelected) return false;

        var interactor = batteryGrab.firstInteractorSelecting;
        if (interactor == null) return false;

        if (robotOffBatterySocket != null && interactor == robotOffBatterySocket)
            return false;

        return true;
    }

    // -----------------------
    // Socket snap/unsnap
    // -----------------------

    private void SnapBatteryIntoOffSocket()
    {
        if (batteryGrab == null || robotOffBatterySocket == null) return;

        var mgr = batteryGrab.interactionManager;
        if (mgr == null) return;

        if (robotOffBatterySocket.hasSelection) return;

        if (batteryGrab.isSelected)
        {
            var currentInteractor = batteryGrab.firstInteractorSelecting;
            if (currentInteractor != null)
                mgr.SelectExit((IXRSelectInteractor)currentInteractor, (IXRSelectInteractable)batteryGrab);
        }

        mgr.SelectEnter((IXRSelectInteractor)robotOffBatterySocket, (IXRSelectInteractable)batteryGrab);
    }

    private void UnsocketBatteryFromOffSocket()
    {
        if (batteryGrab == null || robotOffBatterySocket == null) return;

        var mgr = batteryGrab.interactionManager;
        if (mgr == null) return;

        if (!batteryGrab.isSelected) return;

        var currentInteractor = batteryGrab.firstInteractorSelecting;
        if (currentInteractor == robotOffBatterySocket)
            mgr.SelectExit((IXRSelectInteractor)robotOffBatterySocket, (IXRSelectInteractable)batteryGrab);
    }

    // -----------------------
    // UI / External hooks
    // -----------------------

    public void NotifyShutdownPressed()
    {
        shutdownPressed = true;
        TryAdvanceNow(idxPressShutdown);
    }

    public void NotifyBatteryPowerOff_Explicit()
    {
        batteryPowerOffDone = true;
        TryAdvanceNow(idxBatteryPowerOff);
    }

    public void NotifyBatteryPlacedOnCharger()
    {
        TryAdvanceNow(idxPlaceOnCharger);
    }

    private void TryAdvanceNow(int expectedIndex)
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != expectedIndex) return;
        Advance();
    }
}
