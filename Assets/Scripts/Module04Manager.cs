using UnityEngine;
using System.Collections;

public class Module04Manager : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private RobotSpeechManager speechManager;

    [Header("Controller Canvas Root (opened via X)")]
    [SerializeField] private GameObject controllerCanvasRoot;

    [Header("Final Task: Wait for Menu (must become active)")]
    [SerializeField] private GameObject finalMenuObject;

    [Header("SafeZone Detection (Green/Yellow/Red)")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectionRadius = 2.0f;
    [SerializeField] private float detectionHeightOffset = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform zoneCenter;

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

    private void Update()
    {
        UpdateZoneState();

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

    private void UpdateZoneState()
    {
        if (playerTransform == null) return;

        Transform c = zoneCenter != null ? zoneCenter : transform;

        Vector3 detectionCenter = c.position + Vector3.up * detectionHeightOffset;
        Collider[] obstacles = Physics.OverlapSphere(detectionCenter, detectionRadius, obstacleLayer);

        bool dangerFound = false;
        for (int i = 0; i < obstacles.Length; i++)
        {
            var col = obstacles[i];
            if (!col.CompareTag("Player") && col.gameObject != c.gameObject)
            {
                dangerFound = true;
                break;
            }
        }

        Vector3 zonePos = c.position;
        Vector3 playerPos = playerTransform.position;
        zonePos.y = 0f;
        playerPos.y = 0f;

        float horizontalDistance = Vector3.Distance(zonePos, playerPos);
        bool playerInside = horizontalDistance <= detectionRadius;

        if (dangerFound) zoneIsGreen = false;
        else if (playerInside) zoneIsGreen = false;
        else zoneIsGreen = true;
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

        // Completed -> hide timer text
        ClearTimerUI();

        // IMPORTANT: Do NOT trigger idle here (can interrupt next speech line).
        // Only stop talking bool so the guide doesn't get stuck in old animations.
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
