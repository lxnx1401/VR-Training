using UnityEngine;
using System.Collections;

public class Module04Manager : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Controller Canvas Root (opened via X)")]
    [SerializeField] private GameObject controllerCanvasRoot;

    [Header("SafeZone Controller (partner script)")]
    [SerializeField] private SafeZoneController safeZoneController;

    private RobotSpeechManager cachedSafeZoneSpeech; // restore later (optional)

    [Header("SafeZone Visual Source (same as SafeZoneController)")]
    [SerializeField] private MeshRenderer zoneRenderer;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material redMaterial;

    private bool zoneIsGreen = true;

    [Header("Task 20s Control")]
    [SerializeField] private float forwardControlSeconds = 20f;

    [Header("Optional Timer Display")]
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private string timerPrefix = "Safe control: ";

    [Header("Animation task safety")]
    [SerializeField] private bool requireSafeDuringAnimationTask = true;

    [Header("Task line indices (0-based)")]
    [SerializeField] private int idxOpenController = 3;
    [SerializeField] private int idxPressForward = 4;
    [SerializeField] private int idxHold20Seconds = 5;
    [SerializeField] private int idxSwitchAnimMode = 6;
    [SerializeField] private int idxPressAnimButtons = 7;

    private bool forwardPressed;
    private bool animModeSwitched;

    private bool wavePressed;
    private bool dancePressed;
    private bool kickPressed;

    private bool hadUnsafeDuringAnimTask;

    private Coroutine controlTimerRoutine;
    private int lastIndex = -999;

    private void Awake()
    {
        // IMPORTANT: prevent partner SafeZoneController from auto-advancing lines in Module 4
        if (safeZoneController != null)
        {
            cachedSafeZoneSpeech = safeZoneController.speechManager;
            safeZoneController.speechManager = null;
        }
    }

    private void OnDisable()
    {
        // Optional restore if this object is reused later
        if (safeZoneController != null)
        {
            safeZoneController.speechManager = cachedSafeZoneSpeech;
        }
    }

    private void Update()
    {
        UpdateZoneFromRenderer();

        if (speechManager == null) return;

        int idx = speechManager.GetCurrentIndex();

        if (idx != lastIndex)
        {
            OnLineChanged(idx);
            lastIndex = idx;
        }

        if (!speechManager.isWaitingForTask) return;

        if (idx == idxOpenController)
        {
            if (controllerCanvasRoot != null && controllerCanvasRoot.activeInHierarchy)
                Advance(idxOpenController);
            return;
        }

        if (idx == idxPressForward)
        {
            if (forwardPressed)
                Advance(idxPressForward);
            return;
        }

        if (idx == idxHold20Seconds)
        {
            if (controlTimerRoutine == null)
                controlTimerRoutine = StartCoroutine(SafeControlTimer());
            return;
        }

        if (idx == idxSwitchAnimMode)
        {
            if (animModeSwitched)
                Advance(idxSwitchAnimMode);
            return;
        }

        if (idx == idxPressAnimButtons)
        {
            if (requireSafeDuringAnimationTask && !zoneIsGreen)
                hadUnsafeDuringAnimTask = true;

            bool allPressed = wavePressed && dancePressed && kickPressed;
            if (allPressed)
            {
                if (!requireSafeDuringAnimationTask || !hadUnsafeDuringAnimTask)
                    Advance(idxPressAnimButtons);
            }
            return;
        }
    }

    private void UpdateZoneFromRenderer()
    {
        if (zoneRenderer == null)
        {
            zoneIsGreen = true;
            return;
        }

        Material m = zoneRenderer.material;
        if (m == null)
        {
            zoneIsGreen = true;
            return;
        }

        if (greenMaterial != null && (m == greenMaterial || m.name.StartsWith(greenMaterial.name)))
        {
            zoneIsGreen = true;
            return;
        }

        if (yellowMaterial != null && (m == yellowMaterial || m.name.StartsWith(yellowMaterial.name)))
        {
            zoneIsGreen = false;
            return;
        }

        if (redMaterial != null && (m == redMaterial || m.name.StartsWith(redMaterial.name)))
        {
            zoneIsGreen = false;
            return;
        }

        zoneIsGreen = true;
    }

    private void OnLineChanged(int newIdx)
    {
        if (newIdx == idxPressForward)
            forwardPressed = false;

        if (newIdx == idxHold20Seconds)
        {
            if (controlTimerRoutine != null) StopCoroutine(controlTimerRoutine);
            controlTimerRoutine = null;
            UpdateTimerUI(0f, forwardControlSeconds);
        }
        else
        {
            ClearTimerUI();
        }

        if (newIdx == idxSwitchAnimMode)
            animModeSwitched = false;

        if (newIdx == idxPressAnimButtons)
        {
            wavePressed = false;
            dancePressed = false;
            kickPressed = false;
            hadUnsafeDuringAnimTask = false;
        }
    }

    private IEnumerator SafeControlTimer()
    {
        float t = 0f;

        while (t < forwardControlSeconds)
        {
            if (!zoneIsGreen) t = 0f;
            else t += Time.deltaTime;

            UpdateTimerUI(t, forwardControlSeconds);

            if (speechManager == null || !speechManager.isWaitingForTask || speechManager.GetCurrentIndex() != idxHold20Seconds)
            {
                controlTimerRoutine = null;
                yield break;
            }

            yield return null;
        }

        controlTimerRoutine = null;
        ClearTimerUI();

        StopGuideTalkingBoolOnly();
        Advance(idxHold20Seconds);
    }

    private void UpdateTimerUI(float current, float total)
    {
        if (timerText == null) return;
        float left = Mathf.Max(0f, total - current);
        timerText.text = timerPrefix + left.ToString("0") + "s";
    }

    private void ClearTimerUI()
    {
        if (timerText == null) return;
        timerText.text = "";
    }

    private void StopGuideTalkingBoolOnly()
    {
        if (speechManager == null) return;
        if (speechManager.animator == null) return;

        if (speechManager.useIsTalkingBool && !string.IsNullOrWhiteSpace(speechManager.isTalkingBool))
            speechManager.animator.SetBool(speechManager.isTalkingBool, false);
    }

    private void Advance(int completedTaskIndex)
    {
        if (speechManager == null) return;

        if (controlTimerRoutine != null)
        {
            StopCoroutine(controlTimerRoutine);
            controlTimerRoutine = null;
        }

        speechManager.UnlockTask();
        speechManager.PlayNextLine();
    }

    // UI Hooks
    public void NotifyForwardPressed()
    {
        forwardPressed = true;
        TryAdvanceNow(idxPressForward);
    }

    public void NotifyAnimationModeSwitched()
    {
        animModeSwitched = true;
        TryAdvanceNow(idxSwitchAnimMode);
    }

    public void NotifyWavePressed() { wavePressed = true; }
    public void NotifyDancePressed() { dancePressed = true; }
    public void NotifyKickPressed() { kickPressed = true; }

    private void TryAdvanceNow(int expectedIndex)
    {
        if (speechManager == null) return;
        if (!speechManager.isWaitingForTask) return;
        if (speechManager.GetCurrentIndex() != expectedIndex) return;
        Advance(expectedIndex);
    }
}
