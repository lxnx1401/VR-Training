using UnityEngine;

public class XRToggleAnimatorBool : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName = "Pressed";
    [SerializeField] private float cooldown = 0.15f;

    private float lastTime = -999f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    // an XR Simple Interactable -> Activated() hängen
    public void Toggle()
    {
        if (animator == null) return;
        if (Time.time - lastTime < cooldown) return;
        lastTime = Time.time;

        bool current = animator.GetBool(boolName);
        animator.SetBool(boolName, !current);
    }
}
