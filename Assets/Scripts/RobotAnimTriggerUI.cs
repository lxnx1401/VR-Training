using UnityEngine;

public class RobotAnimTriggerUI : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] private Animator animator; // Animator am Roboter (RobotModel)

    [Header("Trigger Names (müssen exakt so im Animator existieren)")]
    [SerializeField] private string waveTrigger = "Wave";
    [SerializeField] private string kickTrigger = "Kick";
    [SerializeField] private string danceTrigger = "Dance";

    private void Awake()
    {
        // Falls du es nicht im Inspector ziehst, versucht er es automatisch zu finden
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (!animator)
            Debug.LogError("[RobotAnimTriggerUI] Kein Animator gefunden/zugewiesen!");
    }

    // Diese Methoden kannst du direkt im Button OnClick auswählen
    public void PlayWave()
    {
        if (!animator) return;
        animator.SetTrigger(waveTrigger);
    }

    public void PlayKick()
    {
        if (!animator) return;
        animator.SetTrigger(kickTrigger);
    }

    public void PlayDance()
    {
        if (!animator) return;
        animator.SetTrigger(danceTrigger);
    }
}
