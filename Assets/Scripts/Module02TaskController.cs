using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Module02TaskController : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Task line indices (0-based)")]
    [SerializeField] private int task1ArmUpIndex = 4;
    [SerializeField] private int task2BatteryGrabIndex = 5;
    [SerializeField] private int task3BatteryInsertIndex = 6;
    [SerializeField] private int task4PowerPressIndex = 7;
    [SerializeField] private int task5ArmDownIndex = 8;

    [Header("XR References")]
    [SerializeField] private XRSimpleInteractable leftShoulderInteractable;
    [SerializeField] private XRGrabInteractable batteryGrab;

    [Tooltip("XRSocketInteractor on the robot battery slot (recommended)")]
    [SerializeField] private XRSocketInteractor batterySocket;

    [Header("Battery Animator (INT state)")]
    [SerializeField] private Animator batteryAnimator;

    [Tooltip("INT parameter name (example: BatteryState)")]
    [SerializeField] private string batteryStateIntName = "BatteryState";

    [Tooltip("Battery counts as ON when BatteryState >= this value (example: 2 for OnIdle)")]
    [SerializeField] private int onStateMinValue = 2;

    [Header("Arm State")]
    [Tooltip("Fallback toggle if you don't use animation events.")]
    [SerializeField] private bool armIsUp = false;
    [SerializeField] private bool armIsDown = true;

    // One-shot flags
    private bool doneTask1, doneTask2, doneTask3, doneTask4, doneTask5;

    // line tracking + edge detection for Task 4
    private int lastIndex = -999;
    private bool wasOnWhenEnteringTask4;

    private void OnEnable()
    {
        if (leftShoulderInteractable != null)
            leftShoulderInteractable.selectEntered.AddListener(OnShoulderSelected);

        if (batteryGrab != null)
            batteryGrab.selectEntered.AddListener(OnBatteryGrabbed);

        if (batterySocket != null)
            batterySocket.selectEntered.AddListener(OnBatteryInserted);
    }

    private void OnDisable()
    {
        if (leftShoulderInteractable != null)
            leftShoulderInteractable.selectEntered.RemoveListener(OnShoulderSelected);

        if (batteryGrab != null)
            batteryGrab.selectEntered.RemoveListener(OnBatteryGrabbed);

        if (batterySocket != null)
            batterySocket.selectEntered.RemoveListener(OnBatteryInserted);
    }

    private void Update()
    {
        if (speechManager == null) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx != lastIndex)
        {
            OnLineChanged(idx);
            lastIndex = idx;
        }

        if (!speechManager.isWaitingForTask) return;

        // Task 1: arm up
        if (idx == task1ArmUpIndex && !doneTask1 && armIsUp)
        {
            doneTask1 = true;
            Advance();
            return;
        }

        // Task 2: battery grabbed
        if (idx == task2BatteryGrabIndex && !doneTask2)
        {
            if (batteryGrab != null && batteryGrab.isSelected)
            {
                doneTask2 = true;
                Advance();
                return;
            }
        }

        // Task 3: battery inserted
        if (idx == task3BatteryInsertIndex && !doneTask3)
        {
            if (IsBatteryInSocket())
            {
                doneTask3 = true;
                Advance();
                return;
            }
        }

        // Task 4: power button pressed (INT state changes OFF -> ON)
        if (idx == task4PowerPressIndex && !doneTask4)
        {
            if (!IsBatteryInSocket()) return;

            bool isOnNow = IsBatteryOnByStateInt();

            // advance only when we detect OFF -> ON transition on this line
            if (!wasOnWhenEnteringTask4 && isOnNow)
            {
                doneTask4 = true;
                Advance();
                return;
            }
            return;
        }

        // Task 5: arm down (only after power ON)
        if (idx == task5ArmDownIndex && !doneTask5)
        {
            if (IsBatteryInSocket() && IsBatteryOnByStateInt() && armIsDown)
            {
                doneTask5 = true;
                Advance();
                return;
            }
        }
    }

    private void OnLineChanged(int newIdx)
    {
        // Snapshot current ON status when we ENTER Task 4
        if (newIdx == task4PowerPressIndex)
            wasOnWhenEnteringTask4 = IsBatteryOnByStateInt();
        else
            wasOnWhenEnteringTask4 = false;
    }

    private bool IsBatteryInSocket()
    {
        if (batterySocket != null)
            return batterySocket.hasSelection;

        return true; // fallback
    }

    private bool IsBatteryOnByStateInt()
    {
        if (batteryAnimator == null) return false;
        if (string.IsNullOrWhiteSpace(batteryStateIntName)) return false;

        int s = batteryAnimator.GetInteger(batteryStateIntName);
        return s >= onStateMinValue;
    }

    private void Advance()
    {
        speechManager.UnlockTask();
        speechManager.PlayNextLine();
    }

    // -----------------------
    // XR events
    // -----------------------

    private void OnShoulderSelected(SelectEnterEventArgs args)
    {
        // Fallback toggle (best is animation events SetArmUp/SetArmDown)
        armIsUp = !armIsUp;
        armIsDown = !armIsUp;

        if (speechManager == null || !speechManager.isWaitingForTask) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx == task1ArmUpIndex && armIsUp && !doneTask1)
        {
            doneTask1 = true;
            Advance();
            return;
        }

        if (idx == task5ArmDownIndex && armIsDown && !doneTask5 && IsBatteryOnByStateInt())
        {
            doneTask5 = true;
            Advance();
            return;
        }
    }

    private void OnBatteryGrabbed(SelectEnterEventArgs args)
    {
        doneTask2 = true;
        TryInstantAdvance(task2BatteryGrabIndex);
    }

    private void OnBatteryInserted(SelectEnterEventArgs args)
    {
        doneTask3 = true;
        TryInstantAdvance(task3BatteryInsertIndex);
    }

    private void TryInstantAdvance(int expectedIndex)
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != expectedIndex) return;
        Advance();
    }

    // Optional: call these from animation events at the END of your arm up/down animations:
    public void SetArmUp()
    {
        armIsUp = true;
        armIsDown = false;
        TryInstantAdvance(task1ArmUpIndex);
    }

    public void SetArmDown()
    {
        armIsUp = false;
        armIsDown = true;
        TryInstantAdvance(task5ArmDownIndex);
    }
}
