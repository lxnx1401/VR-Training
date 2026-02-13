using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable leftShoulderInteractable;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable batteryGrab;

    [Tooltip("XRSocketInteractor on the robot battery slot (recommended)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor batterySocket;

    [Header("Power Button (Animator Bool on the battery)")]
    [SerializeField] private Animator batteryAnimator;
    [SerializeField] private string powerBoolName = "Pressed"; // must match your boolName
    [Tooltip("If true: power is considered ON when bool is true.")]
    [SerializeField] private bool powerOnWhenBoolTrue = true;

    [Header("Arm State")]
    [Tooltip("Fallback toggle if you don't use animation events.")]
    [SerializeField] private bool armIsUp = false;
    [SerializeField] private bool armIsDown = true;

    // One-shot flags
    private bool doneTask1, doneTask2, doneTask3, doneTask4, doneTask5;

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
        if (!speechManager.isWaitingForTask) return;

        int idx = speechManager.GetCurrentIndex();

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

        // Task 4: power pressed (Animator Bool) - only valid if battery is inserted
        if (idx == task4PowerPressIndex && !doneTask4)
        {
            if (IsBatteryInSocket() && IsPowerOnByAnimator())
            {
                doneTask4 = true;
                Advance();
                return;
            }
        }

        // Task 5: arm down (only after power on)
        if (idx == task5ArmDownIndex && !doneTask5)
        {
            if (IsBatteryInSocket() && IsPowerOnByAnimator() && armIsDown)
            {
                doneTask5 = true;
                Advance();
                return;
            }
        }
    }

    private bool IsBatteryInSocket()
    {
        // Recommended path:
        if (batterySocket != null)
            return batterySocket.hasSelection;

        // If you didn't assign the socket, we can't verify insertion � assume true.
        return true;
    }

    private bool IsPowerOnByAnimator()
    {
        if (batteryAnimator == null || string.IsNullOrWhiteSpace(powerBoolName))
            return false;

        bool v = batteryAnimator.GetBool(powerBoolName);
        return powerOnWhenBoolTrue ? v : !v;
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

        if (idx == task5ArmDownIndex && armIsDown && !doneTask5 && IsPowerOnByAnimator())
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
