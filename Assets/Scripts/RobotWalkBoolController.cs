using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotWalkBoolController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator; // Animator am RobotModel (Child)

    [Header("Animator Bool Names")]
    [SerializeField] private string walkF = "WalkF";
    [SerializeField] private string walkB = "WalkB";
    [SerializeField] private string walkL = "WalkL";
    [SerializeField] private string walkR = "WalkR";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.0f; // m/s

    private Rigidbody rb;
    private Vector3 moveDir; // world direction

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // Stabilität
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (moveDir.sqrMagnitude < 0.0001f) return;

        Vector3 step = moveDir.normalized * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + step);
    }

    // --- Helper: immer nur eine Richtung aktiv ---
    private void SetWalkState(bool f, bool b, bool l, bool r, Vector3 dir)
    {
        if (!animator) return;

        animator.SetBool(walkF, f);
        animator.SetBool(walkB, b);
        animator.SetBool(walkL, l);
        animator.SetBool(walkR, r);

        moveDir = dir;
    }

    // ===== UI Events (PointerDown) =====
    public void ForwardDown() => SetWalkState(true,  false, false, false, transform.forward);
    public void BackDown()    => SetWalkState(false, true,  false, false, -transform.forward);
    public void LeftDown()    => SetWalkState(false, false, true,  false, -transform.right);
    public void RightDown()   => SetWalkState(false, false, false, true,  transform.right);

    // ===== UI Event (PointerUp) =====
    public void StopMove()
    {
        if (!animator) return;

        animator.SetBool(walkF, false);
        animator.SetBool(walkB, false);
        animator.SetBool(walkL, false);
        animator.SetBool(walkR, false);

        moveDir = Vector3.zero;
    }
}
