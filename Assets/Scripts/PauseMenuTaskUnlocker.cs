using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PauseMenuTaskUnlocker : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Dein RobotSpeechManager in der Scene (Ruby). Wenn leer, wird er automatisch gesucht.")]
    public RobotSpeechManager speechManager;

    [Tooltip("Das Pause-Menü-Root-GameObject (z.B. pauseMenuGroup), das aktiv/inaktiv geschaltet wird.")]
    public GameObject pauseMenuRoot;

    [Header("Input (optional)")]
    [Tooltip("Wenn du schon eine InputActionReference für B/Menu hast, hier reinziehen. Sonst nutzt es Fallback auf Keyboard.")]
    public InputActionReference pauseToggleAction;

    [Header("Task Settings")]
    [Tooltip("Nur dann entsperren, wenn Pause-Menü wirklich einmal geöffnet UND wieder geschlossen wurde.")]
    public bool requireOpenAndClose = true;

    [Header("Auto Continue")]
    [Tooltip("Wenn TRUE: Nach erfolgreicher Task wird automatisch die nächste DialogueLine gestartet (ohne Next drücken).")]
    public bool autoAdvanceAfterUnlock = true;

    [Tooltip("Kleine Verzögerung bevor auto-advance passiert (verhindert Doppel-Trigger im selben Frame).")]
    public float autoAdvanceDelay = 0.15f;

    private bool taskArmed = false;
    private bool sawOpen = false;
    private bool unlockingNow = false;

    private void Awake()
    {
        if (!speechManager) speechManager = FindFirstObjectByType<RobotSpeechManager>();
        if (!pauseMenuRoot)
            Debug.LogWarning("[PauseMenuTaskUnlocker] pauseMenuRoot ist nicht gesetzt! Bitte dein Pause-Menü Panel hier zuweisen.");
    }

    private void OnEnable()
    {
        if (pauseToggleAction != null)
            pauseToggleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (pauseToggleAction != null)
            pauseToggleAction.action.Disable();
    }

    private void Update()
    {
        if (!speechManager || unlockingNow) return;

        // Task nur "armen", wenn Ruby wirklich wartet
        if (speechManager.isWaitingForTask && !taskArmed)
        {
            taskArmed = true;
            sawOpen = false;
        }

        // Wenn Ruby nicht mehr wartet, resetten
        if (!speechManager.isWaitingForTask && taskArmed)
        {
            taskArmed = false;
            sawOpen = false;
        }

        if (!taskArmed) return;

        // B/Menu gedrückt?
        bool pressed =
            (pauseToggleAction != null && pauseToggleAction.action.triggered) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);

        if (!pressed) return;

        // Status vom Pause-Menü ablesen (aktiv = offen)
        bool isOpenNow = pauseMenuRoot != null && pauseMenuRoot.activeInHierarchy;

        if (!requireOpenAndClose)
        {
            StartCoroutine(UnlockAndMaybeAdvance());
            return;
        }

        // Logik: erst öffnen sehen, dann schließen sehen
        if (isOpenNow)
        {
            sawOpen = true;
        }
        else
        {
            if (sawOpen)
            {
                StartCoroutine(UnlockAndMaybeAdvance());
            }
        }
    }

    private IEnumerator UnlockAndMaybeAdvance()
    {
        unlockingNow = true;

        // 1) Task entsperren
        if (speechManager != null)
            speechManager.UnlockTask();

        // 2) lokale States resetten
        taskArmed = false;
        sawOpen = false;

        // 3) optional automatisch weiter zur nächsten Line 
        if (autoAdvanceAfterUnlock && speechManager != null)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            speechManager.PlayNextLine();
        }

        unlockingNow = false;
    }
}
