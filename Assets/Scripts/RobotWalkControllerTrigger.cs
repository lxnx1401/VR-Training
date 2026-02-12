using UnityEngine;
using System.Collections;

public class RobotWalkControllerTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform robotRoot;
    public Animator animator;

    [Header("Movement")]
    public float defaultSpeed = 1.2f;
    public float turnSpeed = 360f;
    public float stopDistance = 0.35f;
    public bool ignoreY = true;

    private Coroutine walkRoutine;

    void Awake()
    {
        if (!robotRoot) robotRoot = transform;
    }

    public void StartWalkToTargetWhileAudio(AudioSource source, Transform target, string walkTrigger, float speedOverride = 0f)
    {
        if (source == null) { Debug.LogWarning("[WalkTrigger] AudioSource ist null."); return; }
        if (target == null) { Debug.LogWarning("[WalkTrigger] Target ist null."); return; }
        if (!animator) { Debug.LogWarning("[WalkTrigger] Animator ist nicht gesetzt."); return; }

        StopWalk();

        // Walk-Animation starten (Trigger-only)
        if (!string.IsNullOrWhiteSpace(walkTrigger))
            animator.SetTrigger(walkTrigger);

        float speed = (speedOverride > 0f) ? speedOverride : defaultSpeed;
        walkRoutine = StartCoroutine(WalkToTargetWhileAudioRoutine(source, target, speed));
    }

    public void StopWalk()
    {
        if (walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
       
    }

    private IEnumerator WalkToTargetWhileAudioRoutine(AudioSource source, Transform target, float speed)
    {
        while (source != null && source.isPlaying && target != null)
        {
            Vector3 pos = robotRoot.position;
            Vector3 dest = target.position;
            if (ignoreY) dest.y = pos.y;

            Vector3 toTarget = dest - pos;
            float dist = toTarget.magnitude;
            if (dist <= stopDistance) break;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                robotRoot.rotation = Quaternion.RotateTowards(robotRoot.rotation, look, turnSpeed * Time.deltaTime);
            }

            robotRoot.position = Vector3.MoveTowards(pos, dest, speed * Time.deltaTime);

            yield return null;
        }

        
        walkRoutine = null;
    }
}
