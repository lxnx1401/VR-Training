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

    [Header("Task line indices (0-based)")]
    [SerializeField] private int idxPressShutdown = 3;
    [SerializeField] private int idxCloseController = 4;
    [SerializeField] private int idxLiftArm = 5;
    [SerializeField] private int idxBatteryPowerOff = 6;
    [SerializeField] private int idxRemoveBattery = 7;
    [SerializeField] private int idxPlaceOnCharger = 8;

    // --- task flags ---
    private bool shutdownPressed;
    private bool armLifted;
    private bool batteryPoweredOff;
    private bool batteryPlacedOnCharger;

    private bool swappedToOffRobot;
    private int lastIdx = -999;

    private void Update()
    {
        if (speechManager == null) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx != lastIdx)
        {
            OnLineChanged(idx);
            lastIdx = idx;
        }

        if (!speechManager.isWaitingForTask) return;

        if (idx == idxPressShutdown)
        {
            if (shutdownPressed) Advance();
            return;
        }

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

                Advance();
            }
            return;
        }

        if (idx == idxLiftArm)
        {
            if (armLifted) Advance();
            return;
        }

        if (idx == idxBatteryPowerOff)
        {
            if (batteryPoweredOff) Advance();
            return;
        }

        if (idx == idxRemoveBattery)
        {
            if (!batteryPoweredOff) return;

            // Removed = held by hand (not socket)
            if (IsBatteryHeldByHand())
                Advance();

            return;
        }

        if (idx == idxPlaceOnCharger)
        {
            if (batteryPlacedOnCharger) Advance();
            return;
        }
    }

    private void OnLineChanged(int newIdx)
    {
        if (newIdx == idxRemoveBattery)
        {
            UnsocketBatteryFromOffSocket();
        }
    }

    private void Advance()
    {
        speechManager.UnlockTask();
        speechManager.PlayNextLine();
    }

    private void SwapRobotOnToOff_CopyXZOnly()
    {
        if (robotOn == null || robotOff == null) return;

        Vector3 offPos = robotOff.position; // keep OFF Y as-is
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
    // Battery helpers (XR)
    // -----------------------

    private bool IsBatteryHeldByHand()
    {
        if (batteryGrab == null) return false;
        if (!batteryGrab.isSelected) return false;

        var interactor = batteryGrab.firstInteractorSelecting;
        if (interactor == null) return false;

        // If socket is holding it, it's not removed
        if (robotOffBatterySocket != null && interactor == robotOffBatterySocket)
            return false;

        return true;
    }

    private void SnapBatteryIntoOffSocket()
    {
        if (batteryGrab == null || robotOffBatterySocket == null) return;

        var mgr = batteryGrab.interactionManager;
        if (mgr == null) return;

        // If socket already holds something, don't fight
        if (robotOffBatterySocket.hasSelection) return;

        // If battery is currently selected, release it first
        if (batteryGrab.isSelected)
        {
            var currentInteractor = batteryGrab.firstInteractorSelecting;
            if (currentInteractor != null)
            {
                mgr.SelectExit((IXRSelectInteractor)currentInteractor, (IXRSelectInteractable)batteryGrab);
            }
        }

        // Snap into OFF socket using new API (interfaces)
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
        {
            mgr.SelectExit((IXRSelectInteractor)robotOffBatterySocket, (IXRSelectInteractable)batteryGrab);
        }
    }

    // -----------------------
    // UI / Interaction Hooks
    // -----------------------

    public void NotifyShutdownPressed()
    {
        shutdownPressed = true;
        TryAdvanceNow(idxPressShutdown);
    }

    public void NotifyArmLifted()
    {
        armLifted = true;
        TryAdvanceNow(idxLiftArm);
    }

    public void NotifyBatteryPowerOff()
    {
        batteryPoweredOff = true;
        TryAdvanceNow(idxBatteryPowerOff);
    }

    // Call from CHARGER socket selectEntered OR your charger logic
    public void NotifyBatteryPlacedOnCharger()
    {
        batteryPlacedOnCharger = true;
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
