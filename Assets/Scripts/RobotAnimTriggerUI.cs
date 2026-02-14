// RobotAnimTriggerUI.cs
using UnityEngine;

public class RobotAnimTriggerUI : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] private Animator animator;

    [Header("Trigger Names (must exist in Animator)")]
    [SerializeField] private string waveTrigger = "Wave";
    [SerializeField] private string kickTrigger = "Kick";
    [SerializeField] private string danceTrigger = "Dance";

    private void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (!animator)
            Debug.LogError("[RobotAnimTriggerUI] No Animator found/assigned!");
    }

    // These methods can be assigned in Button OnClick
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
