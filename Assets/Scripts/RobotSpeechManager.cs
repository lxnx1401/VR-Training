using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class RobotSpeechManager : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public AudioClip audioClip;
        [TextArea(3, 10)]
        public string subtitle;
    }

    public List<DialogueLine> dialogueLines;
    private int currentIndex = -1;
    private bool isTyping = false;
    public bool isWaitingForTask = false; // SafeZoneController buradan kilidi açacak

    public TextMeshProUGUI subtitleText;
    public GameObject subtitleGroup;
    public AudioSource audioSource;
    public float typeSpeed = 0.05f;
    public InputActionReference nextLineAction; 

    void Start()
    {
        Invoke("PlayNextLine", 1.5f);
    }

    void Update()
    {
        // Space veya VR butonu
        bool nextPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || 
                           (nextLineAction != null && nextLineAction.action.triggered);

        if (nextPressed)
        {
            if (isTyping)
            {
                StopAllCoroutines();
                isTyping = false;
                subtitleText.text = dialogueLines[currentIndex].subtitle;
            }
            else
            {
                // EĞER KİLİT VARSA NEXT ÇALIŞMAZ
                if (!isWaitingForTask) 
                {
                    PlayNextLine();
                }
            }
        }
    }

    public void PlayNextLine()
    {
        currentIndex++;

        if (currentIndex < dialogueLines.Count)
        {
            // EĞER 3. CÜMLEYE GELDİYSEK (Index 2), KİLİDİ AKTİF ET
            if (currentIndex == 2) 
            {
                isWaitingForTask = true;
            }

            StopAllCoroutines();
            StartCoroutine(TypeTextRoutine(dialogueLines[currentIndex]));
        }
        else
        {
            subtitleGroup.SetActive(false);
            subtitleText.text = "";
        }
    }

    private IEnumerator TypeTextRoutine(DialogueLine line)
    {
        isTyping = true;
        subtitleGroup.SetActive(true);
        subtitleText.text = "";

        if (audioSource != null && line.audioClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.audioClip;
            audioSource.Play();
        }

        foreach (char letter in line.subtitle.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    public int GetCurrentIndex() { return currentIndex; }
}