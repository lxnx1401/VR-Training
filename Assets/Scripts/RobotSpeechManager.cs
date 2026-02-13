using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // <-- NEU (Scene-Wechsel)

public class RobotSpeechManager : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public AudioClip audioClip;

        [TextArea(3, 10)]
        public string subtitle;

        [Header("Trigger (Talk ODER Walk)")]
        public string talkTrigger; // z.B. "Talk" ODER "Walk"

        [Header("Task (optional)")]
        [Tooltip("Wenn TRUE: Nach dieser Zeile wird Next gesperrt, bis UnlockTask() aufgerufen wird.")]
        public bool waitForTaskUnlock;
    }

    public List<DialogueLine> dialogueLines;

    private int currentIndex = -1;
    private bool isTyping = false;

    public bool isWaitingForTask = false;

    [Header("UI - Bauchbinde")]
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI speakerNameText; // <-- YENİ: Buraya "RUBI" yazan Text'i bağla
    public GameObject subtitleGroup; // <-- Bu senin "Bauchbinde" (Gri/Siyah Panel)

    [Header("Audio")]
    public AudioSource audioSource;
    public float typeSpeed = 0.05f;

    [Header("Input")]
    public InputActionReference nextLineAction;

    [Header("Animator")]
    public Animator animator;
    public string isTalkingBool = "IsTalking";
    public bool useIsTalkingBool = true;

    public string defaultTalkTrigger = "Talk";
    public string idleTrigger = "Idle";

    [Header("Walk (Trigger-only)")]
    public RobotWalkControllerTrigger walkController;
    public string walkTriggerName = "Walk";
    public float walkSpeed = 1.2f;
    public Transform walkTarget;

    [Header("Skip Behaviour")]
    public bool skipAlsoStopsAudio = true;
    public bool skipAlsoStopsWalk = true;

    [Header("Next Interrupt Behaviour")]
    public bool nextInterruptsCurrentLine = true;
    public bool nextAlsoAdvancesAfterInterrupt = true;

    [Header("Scene Wechsel (nach letztem Next)")]
    [Tooltip("Wenn gesetzt: Nach der letzten Zeile und erneutem Next wird diese Scene geladen.")]
    public string nextSceneName = ""; // <-- NEU: Hier Scene-Name eintragen (muss in Build Settings sein)

    private Coroutine audioAnimRoutine;
    private Coroutine typingRoutine;

    void Start()
    {
        // Başlangıçta ismi ayarla

        Invoke(nameof(PlayNextLine), 1.5f);
    }

    void Update()
    {
        bool nextPressed =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (nextLineAction != null && nextLineAction.action.triggered);

        // Tuşa basıldığında OnNextInteraction'ı çağır
        if (nextPressed)
        {
            OnNextInteraction();
        }
    }

    /// <summary>
    /// Bu fonksiyonu hem Update (Tuşlar) çağırıyor,
    /// hem de yeni oluşturduğun "Buton (OnClick)" olayına bağlayabilirsin.
    /// </summary>
    public void OnNextInteraction()
    {
        // Eğer gesperrt: nichts machen (Task muss erst UnlockTask() rufen)
        if (isWaitingForTask) return;

        // Skip typing
        if (isTyping)
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = null;
            isTyping = false;

            if (currentIndex >= 0 && currentIndex < dialogueLines.Count)
                subtitleText.text = dialogueLines[currentIndex].subtitle;

            if (skipAlsoStopsAudio)
                InterruptCurrentLineToIdle();

            return;
        }

        // Interrupt laufende Audio mit Next (optional)
        if (nextInterruptsCurrentLine && audioSource != null && audioSource.isPlaying)
        {
            InterruptCurrentLineToIdle();

            if (nextAlsoAdvancesAfterInterrupt)
                PlayNextLine();

            return;
        }

        // normal next
        PlayNextLine();
    }

    public void PlayNextLine()
    {
        currentIndex++;

        if (currentIndex < dialogueLines.Count)
        {
            StopLineRoutinesOnly();
            typingRoutine = StartCoroutine(TypeTextRoutine(dialogueLines[currentIndex]));
        }
        else
        {
            subtitleGroup.SetActive(false);
            subtitleText.text = "";
            StopTalkingAnimation();
            TriggerIdle();

            if (walkController != null) walkController.StopWalk();

            // <-- NEU: Scene wechseln nach dem letzten "Next"
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    private IEnumerator TypeTextRoutine(DialogueLine line)
    {
        isTyping = true;
        subtitleGroup.SetActive(true);
        subtitleText.text = "";

        // ✅ Task-Lock pro Zeile
        if (line.waitForTaskUnlock)
            isWaitingForTask = true;

        if (audioSource != null && line.audioClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.audioClip;
            audioSource.loop = false;
            audioSource.Play();

            string trigger = string.IsNullOrWhiteSpace(line.talkTrigger) ? defaultTalkTrigger : line.talkTrigger;

            if (trigger == walkTriggerName)
            {
                StopTalkingAnimation();

                if (walkController != null)
                    walkController.StartWalkToTargetWhileAudio(audioSource, walkTarget, walkTriggerName, walkSpeed);
            }
            else
            {
                StartTalkingAnimation(trigger);

                if (walkController != null)
                    walkController.StopWalk();
            }

            if (audioAnimRoutine != null) StopCoroutine(audioAnimRoutine);
            audioAnimRoutine = StartCoroutine(WaitForAudioThenStop(trigger));
        }
        else
        {
            StopTalkingAnimation();
            TriggerIdle();
            if (walkController != null) walkController.StopWalk();
        }

        foreach (char letter in line.subtitle)
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void StartTalkingAnimation(string trigger)
    {
        if (!animator) return;

        if (!string.IsNullOrWhiteSpace(trigger))
            animator.SetTrigger(trigger);

        if (useIsTalkingBool && !string.IsNullOrWhiteSpace(isTalkingBool) && trigger != walkTriggerName)
            animator.SetBool(isTalkingBool, true);
    }

    private IEnumerator WaitForAudioThenStop(string modeTrigger)
    {
        yield return null;

        while (audioSource != null && audioSource.isPlaying)
            yield return null;

        StopTalkingAnimation();
        TriggerIdle();

        if (walkController != null && modeTrigger == walkTriggerName)
            walkController.StopWalk();

        audioAnimRoutine = null;
    }

    private void StopTalkingAnimation()
    {
        if (!animator) return;

        if (useIsTalkingBool && !string.IsNullOrWhiteSpace(isTalkingBool))
            animator.SetBool(isTalkingBool, false);
    }

    private void TriggerIdle()
    {
        if (animator && !string.IsNullOrWhiteSpace(idleTrigger))
            animator.SetTrigger(idleTrigger);
    }

    private void InterruptCurrentLineToIdle()
    {
        if (audioAnimRoutine != null) StopCoroutine(audioAnimRoutine);
        audioAnimRoutine = null;

        if (audioSource != null)
            audioSource.Stop();

        StopTalkingAnimation();
        TriggerIdle();

        if (skipAlsoStopsWalk && walkController != null)
            walkController.StopWalk();
    }

    private void StopLineRoutinesOnly()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = null;

        if (audioAnimRoutine != null) StopCoroutine(audioAnimRoutine);
        audioAnimRoutine = null;
    }

    public void UnlockTask()
    {
        isWaitingForTask = false;
    }

    public int GetCurrentIndex() { return currentIndex; }
}
