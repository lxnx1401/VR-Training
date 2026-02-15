using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Module05Manager : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Controller Canvas (opened via X)")]
    [SerializeField] private GameObject controllerCanvasRoot;

    [Header("Optional: Partner SafeZoneController (prevent auto-advance)")]
    [SerializeField] private SafeZoneController safeZoneController;
    private RobotSpeechManager cachedSafeZoneSpeech;

    [Header("Robot Fault Animation (zapel)")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private string zapelBoolName = "zapel";

    [Header("Timing")]
    [Tooltip("How long the player controls before the fault starts (seconds).")]
    [SerializeField] private float controlSecondsBeforeFault = 10f;

    [Tooltip("How long the player has to press NOT-STOP after the fault starts (seconds).")]
    [SerializeField] private float reactionWindowSeconds = 2.5f;

    [Header("Task line indices (0-based)")]
    [SerializeField] private int idxOpenAndConfirm = 3;
    [SerializeField] private int idxAnimTwoActions = 4;
    [SerializeField] private int idxSwitchToControl = 5;
    [SerializeField] private int idxControlPhase = 6;
    [SerializeField] private int idxHitNotStop = 7;
    [SerializeField] private int idxSuccess = 8;

    [Header("Fail handling (optional)")]
    [SerializeField] private bool useFailLine = true;
    [Tooltip("Index of the FAIL sentence line.")]
    [SerializeField] private int idxFailLine = 9;

    [Header("Fail Line Animation (Guide/Ruby)")]
    [Tooltip("Trigger name in Ruby/Guide animator to play when FAIL line is reached. Leave empty to disable.")]
    [SerializeField] private string failLineTrigger = "Fail";
    [SerializeField] private bool failLineAlsoStopsTalkingBool = true;

    [Header("Next module / scene")]
    [Tooltip("Scene name to load after SUCCESS line audio ends.")]
    [SerializeField] private string nextSceneName = "";

    // --- task flags / state ---
    private bool confirmed;
    private bool controlModeSwitched;

    // Animation task (Module04-style)
    private bool wavePressed;
    private bool dancePressed;
    private bool kickPressed;

    // Control / fault state
    private bool controlStarted;
    private float controlStartTime;

    private bool faultActive;
    private float faultStartTime;

    private bool notStopPressed;

    private bool passed;
    private bool failing;

    private Coroutine controlRoutine;
    private Coroutine endRoutine;

    private int lastIndex = -999;

    private void Awake()
    {
        // Prevent partner SafeZoneController from auto-advancing speech in this module
        if (safeZoneController != null)
        {
            cachedSafeZoneSpeech = safeZoneController.speechManager;
            safeZoneController.speechManager = null;
        }
    }

    private void OnDisable()
    {
        if (safeZoneController != null)
            safeZoneController.speechManager = cachedSafeZoneSpeech;
    }

    private void Update()
    {
        if (speechManager == null) return;

        int idx = speechManager.GetCurrentIndex();

        // Reset flags on line change (Module04-style)
        if (idx != lastIndex)
        {
            OnLineChanged(idx);
            lastIndex = idx;
        }

        // IMPORTANT FIX:
        // SUCCESS handling must run even if isWaitingForTask is FALSE (otherwise scene never loads).
        if (idx == idxSuccess)
        {
            if (passed && endRoutine == null)
                endRoutine = StartCoroutine(WaitSuccessAudioThenLoadNext());
            // do not return; let rest run if waiting for task happens to be true
        }

        // For task logic we still need waiting
        if (!speechManager.isWaitingForTask) return;

        // Line 3: open controller + confirm
        if (idx == idxOpenAndConfirm)
        {
            bool canvasOpen = (controllerCanvasRoot != null && controllerCanvasRoot.activeInHierarchy);
            if (canvasOpen && confirmed)
                Advance();
            return;
        }

        // Line 4: wait for 2 animation actions (any 2 of Wave/Dance/Kick)
        if (idx == idxAnimTwoActions)
        {
            int done = 0;
            if (wavePressed) done++;
            if (dancePressed) done++;
            if (kickPressed) done++;

            if (done >= 2)
                Advance();

            return;
        }

        // Line 5: switch to control mode
        if (idx == idxSwitchToControl)
        {
            if (controlModeSwitched)
                Advance();
            return;
        }

        // Line 6: control phase -> after X seconds trigger fault and go to NOT-STOP line
        if (idx == idxControlPhase)
        {
            if (controlStarted && controlRoutine == null)
                controlRoutine = StartCoroutine(ControlThenFaultFlow());
            return;
        }

        // Line 7: NOT-STOP reaction window
        if (idx == idxHitNotStop)
        {
            if (!faultActive) return;

            float elapsed = Time.time - faultStartTime;

            // too slow => fail
            if (!notStopPressed && elapsed > reactionWindowSeconds)
            {
                TriggerFail();
                return;
            }

            // success
            if (notStopPressed)
            {
                SetZapel(false);
                faultActive = false;

                passed = true;
                Advance(); // goes to success line
            }
            return;
        }
    }

    private void OnLineChanged(int newIdx)
    {
        if (newIdx == idxOpenAndConfirm)
        {
            confirmed = false;
        }

        if (newIdx == idxAnimTwoActions)
        {
            wavePressed = false;
            dancePressed = false;
            kickPressed = false;
        }

        if (newIdx == idxSwitchToControl)
        {
            controlModeSwitched = false;
        }

        if (newIdx == idxControlPhase)
        {
            controlStarted = false;
            controlStartTime = 0f;

            if (controlRoutine != null)
            {
                StopCoroutine(controlRoutine);
                controlRoutine = null;
            }
        }

        if (newIdx == idxHitNotStop)
        {
            notStopPressed = false;
        }
    }

    private IEnumerator ControlThenFaultFlow()
    {
        while (Time.time - controlStartTime < controlSecondsBeforeFault)
        {
            if (speechManager == null || speechManager.GetCurrentIndex() != idxControlPhase)
            {
                controlRoutine = null;
                yield break;
            }
            yield return null;
        }

        // Start fault
        faultActive = true;
        notStopPressed = false;
        faultStartTime = Time.time;

        SetZapel(true);

        // Advance to "Hit NOT-STOP!"
        Advance();

        controlRoutine = null;
    }

    private IEnumerator WaitSuccessAudioThenLoadNext()
    {
        // Wait for current audio end (robust)
        if (speechManager != null && speechManager.audioSource != null)
        {
            // If audio is already playing -> wait until it ends
            if (speechManager.audioSource.isPlaying)
            {
                while (speechManager.audioSource.isPlaying)
                    yield return null;
            }
            else
            {
                // If audio might start slightly later -> wait a short moment
                float t0 = Time.time;
                while (!speechManager.audioSource.isPlaying && Time.time - t0 < 0.5f)
                    yield return null;

                while (speechManager.audioSource.isPlaying)
                    yield return null;
            }
        }

        // unlock if it happens to be locked
        if (speechManager != null && speechManager.isWaitingForTask && speechManager.GetCurrentIndex() == idxSuccess)
            speechManager.UnlockTask();

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);

        endRoutine = null;
    }

    private void TriggerFail()
    {
        if (failing) return;
        failing = true;

        if (controlRoutine != null) { StopCoroutine(controlRoutine); controlRoutine = null; }

        SetZapel(false);
        faultActive = false;

        if (useFailLine && speechManager != null)
        {
            if (endRoutine != null) StopCoroutine(endRoutine);
            endRoutine = StartCoroutine(GoToFailLineThenRestart());
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator GoToFailLineThenRestart()
    {
        if (speechManager == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }

        // Jump forward until we reach idxFailLine
        while (speechManager.GetCurrentIndex() < idxFailLine)
        {
            speechManager.UnlockTask();
            speechManager.PlayNextLine();
            yield return null;

            // IMPORTANT FIX:
            // If we are still not on the fail line, immediately cancel what the intermediate line started
            if (speechManager.GetCurrentIndex() < idxFailLine)
            {
                StopIntermediateLineVisuals();
            }

            // safety
            if (speechManager.GetCurrentIndex() > idxFailLine + 5)
                break;
        }

        // We are now on the fail line -> force fail animation
        if (speechManager.GetCurrentIndex() == idxFailLine)
        {
            PlayFailLineAnimation();
        }

        // Wait for fail audio (if any)
        if (speechManager.audioSource != null)
        {
            float t0 = Time.time;
            while (!speechManager.audioSource.isPlaying && Time.time - t0 < 0.5f)
                yield return null;

            while (speechManager.audioSource.isPlaying)
                yield return null;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void StopIntermediateLineVisuals()
    {
        // Stop audio so intermediate triggers don't feel like they "played"
        if (speechManager != null && speechManager.audioSource != null)
            speechManager.audioSource.Stop();

        // Force Guide/Ruby back to idle to avoid showing the wrong animation
        if (speechManager != null && speechManager.animator != null)
        {
            if (speechManager.useIsTalkingBool && !string.IsNullOrWhiteSpace(speechManager.isTalkingBool))
                speechManager.animator.SetBool(speechManager.isTalkingBool, false);

            if (!string.IsNullOrWhiteSpace(speechManager.idleTrigger))
                speechManager.animator.SetTrigger(speechManager.idleTrigger);
        }

        // Optional: clear subtitle to avoid flashing wrong line
        if (speechManager != null && speechManager.subtitleText != null)
            speechManager.subtitleText.text = "";
    }

    private void PlayFailLineAnimation()
    {
        if (speechManager == null) return;
        if (speechManager.animator == null) return;

        if (failLineAlsoStopsTalkingBool && speechManager.useIsTalkingBool && !string.IsNullOrWhiteSpace(speechManager.isTalkingBool))
            speechManager.animator.SetBool(speechManager.isTalkingBool, false);

        if (!string.IsNullOrWhiteSpace(failLineTrigger))
            speechManager.animator.SetTrigger(failLineTrigger);
    }

    private void SetZapel(bool on)
    {
        if (robotAnimator == null) return;
        if (string.IsNullOrWhiteSpace(zapelBoolName)) return;
        robotAnimator.SetBool(zapelBoolName, on);
    }

    private void Advance()
    {
        if (speechManager == null) return;

        speechManager.UnlockTask();
        speechManager.PlayNextLine();
    }

    // -----------------------
    // UI Hooks (Buttons)
    // -----------------------

    public void NotifyConfirm()
    {
        confirmed = true;
        TryAdvanceNow(idxOpenAndConfirm);
    }

    public void NotifySwitchedToControlMode()
    {
        controlModeSwitched = true;
        TryAdvanceNow(idxSwitchToControl);
    }

    public void NotifyWavePressed() { wavePressed = true; TryAdvanceAnimNow(); }
    public void NotifyDancePressed() { dancePressed = true; TryAdvanceAnimNow(); }
    public void NotifyKickPressed() { kickPressed = true; TryAdvanceAnimNow(); }

    private void TryAdvanceAnimNow()
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != idxAnimTwoActions) return;

        int done = 0;
        if (wavePressed) done++;
        if (dancePressed) done++;
        if (kickPressed) done++;

        if (done >= 2)
            Advance();
    }

    public void NotifyControlStarted()
    {
        if (controlStarted) return;

        controlStarted = true;
        controlStartTime = Time.time;

        if (speechManager != null && speechManager.isWaitingForTask && speechManager.GetCurrentIndex() == idxControlPhase)
        {
            if (controlRoutine == null)
                controlRoutine = StartCoroutine(ControlThenFaultFlow());
        }
    }

    public void NotifyNotStop()
    {
        notStopPressed = true;
        // Update() handles pass/fail with timing.
    }

    private void TryAdvanceNow(int expectedIndex)
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != expectedIndex) return;
        Advance();
    }
}
