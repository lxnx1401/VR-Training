using UnityEngine;
using System.Collections;

public class Module03Manager : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Task 1: Controller Canvas (opened via X)")]
    [SerializeField] private GameObject controllerCanvasRoot;

    [Header("Final Task: Wait for Menu (must become active)")]
    [SerializeField] private GameObject finalMenuObject;

    [Header("Robot Zapel Animator")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private string zapelBoolName = "zapel";

    [Header("Zapel Timing")]
    [SerializeField] private float delayAfterStart = 5.0f;

    [Header("Task line indices (0-based)")]
    [SerializeField] private int taskOpenXIndex = 3;
    [SerializeField] private int taskConfirmIndex = 4;
    [SerializeField] private int taskStartIndex = 5;
    [SerializeField] private int taskObserveIndex = 6;
    [SerializeField] private int taskEmergencyIndex = 7;
    [SerializeField] private int taskRestartIndex = 8;
    [SerializeField] private int taskWaitMenuIndex = 9;

    [Header("Idle reset behavior")]
    [Tooltip("If true, idle will only be triggered after Start/Observe lines. Confirm/E-Stop/Restart will NOT trigger idle.")]
    [SerializeField] private bool idleOnlyOnStartAndObserve = true;

    private bool confirmed;
    private bool started;
    private bool emergencyPressed;
    private bool restartPressed;

    private Coroutine zapelRoutine;
    private Coroutine idleAfterAudioRoutine;

    private void Update()
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx == taskOpenXIndex)
        {
            if (controllerCanvasRoot != null && controllerCanvasRoot.activeInHierarchy)
                Advance();
            return;
        }

        if (idx == taskConfirmIndex)
        {
            if (confirmed) Advance();
            return;
        }

        if (idx == taskStartIndex)
        {
            if (started) Advance();
            return;
        }

        if (idx == taskObserveIndex)
        {
            if (started) Advance();
            return;
        }

        if (idx == taskEmergencyIndex)
        {
            if (emergencyPressed) Advance();
            return;
        }

        if (idx == taskRestartIndex)
        {
            if (restartPressed) Advance();
            return;
        }

        if (idx == taskWaitMenuIndex)
        {
            if (finalMenuObject != null && finalMenuObject.activeInHierarchy)
                Advance();
            return;
        }
    }

    private void Advance()
    {
        if (speechManager == null) return;

        int oldIdx = speechManager.GetCurrentIndex();

        speechManager.UnlockTask();
        speechManager.PlayNextLine();

        // Only do idle reset after Start/Observe tasks (not confirm/e-stop/restart)
        if (idleOnlyOnStartAndObserve)
        {
            if (oldIdx == taskStartIndex || oldIdx == taskObserveIndex)
            {
                StartIdleAfterAudio();
            }
        }
        else
        {
            // If you ever want it globally again
            StartIdleAfterAudio();
        }
    }

    private void StartIdleAfterAudio()
    {
        if (idleAfterAudioRoutine != null) StopCoroutine(idleAfterAudioRoutine);
        idleAfterAudioRoutine = StartCoroutine(WaitForLineAudioThenIdle());
    }

    // ---------------------------
    // UI Button hooks (OnClick)
    // ---------------------------

    public void NotifyConfirm()
    {
        confirmed = true;
        TryAdvanceNow(taskConfirmIndex);
    }

    public void NotifyStart()
    {
        started = true;

        if (zapelRoutine != null) StopCoroutine(zapelRoutine);
        zapelRoutine = StartCoroutine(SetZapelTrueAfterDelay());

        TryAdvanceNow(taskStartIndex);
        TryAdvanceNow(taskObserveIndex);
    }

    public void NotifyEmergencyStop()
    {
        emergencyPressed = true;

        SetZapel(false);

        if (zapelRoutine != null) StopCoroutine(zapelRoutine);
        zapelRoutine = null;

        TryAdvanceNow(taskEmergencyIndex);
    }

    public void NotifyRestartOrShutdown()
    {
        restartPressed = true;

        started = false;
        emergencyPressed = false;

        SetZapel(false);

        if (zapelRoutine != null) StopCoroutine(zapelRoutine);
        zapelRoutine = null;

        TryAdvanceNow(taskRestartIndex);
    }

    // ---------------------------
    // Zapel timing
    // ---------------------------

    private IEnumerator SetZapelTrueAfterDelay()
    {
        yield return new WaitForSeconds(delayAfterStart);

        if (!emergencyPressed)
            SetZapel(true);

        zapelRoutine = null;
    }

    private void SetZapel(bool on)
    {
        if (robotAnimator == null) return;
        if (string.IsNullOrWhiteSpace(zapelBoolName)) return;
        robotAnimator.SetBool(zapelBoolName, on);
    }

    // ---------------------------
    // Idle after audio
    // ---------------------------

    private IEnumerator WaitForLineAudioThenIdle()
    {
        if (speechManager == null) yield break;

        var a = speechManager.audioSource;
        if (a == null) yield break;

        // Wait a bit for audio to start
        float t0 = Time.time;
        while (!a.isPlaying && Time.time - t0 < 1.0f)
            yield return null;

        if (!a.isPlaying) yield break;

        while (a.isPlaying)
            yield return null;

        yield return null;

        ForceGuideIdle();
        idleAfterAudioRoutine = null;
    }

    private void ForceGuideIdle()
    {
        if (speechManager == null) return;
        if (speechManager.animator == null) return;

        if (speechManager.useIsTalkingBool && !string.IsNullOrWhiteSpace(speechManager.isTalkingBool))
            speechManager.animator.SetBool(speechManager.isTalkingBool, false);

        if (!string.IsNullOrWhiteSpace(speechManager.idleTrigger))
            speechManager.animator.SetTrigger(speechManager.idleTrigger);
    }

    private void TryAdvanceNow(int expectedIndex)
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != expectedIndex) return;
        Advance();
    }
}
