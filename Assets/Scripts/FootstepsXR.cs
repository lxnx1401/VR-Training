using UnityEngine;

public class FootstepsXR : MonoBehaviour
{
    [Header("Track This Transform (XR Origin oder HMD)")]
    [SerializeField] private Transform trackedTransform;

    [Header("Use this AudioSource (volume is controlled there)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Step Clips")]
    [SerializeField] private AudioClip[] stepClips;

    [Header("Step Settings")]
    [SerializeField] private float stepDistance = 0.55f;
    [SerializeField] private float minSpeed = 0.15f;

    private Vector3 lastPos;
    private float accumDistance;

    private void Awake()
    {
        if (trackedTransform == null) trackedTransform = transform;

        // WICHTIG: keine AudioSource automatisch erstellen – du weist sie zu
        if (audioSource == null)
            Debug.LogWarning($"{nameof(FootstepsXR)}: No AudioSource assigned on {name}. No footsteps will play.");
    }

    private void OnEnable()
    {
        if (trackedTransform == null) trackedTransform = transform;
        lastPos = trackedTransform.position;
        accumDistance = 0f;
    }

    private void Update()
    {
        if (audioSource == null) return;
        if (stepClips == null || stepClips.Length == 0) return;

        Vector3 currentPos = trackedTransform.position;

        // nur horizontal
        Vector3 delta = currentPos - lastPos;
        delta.y = 0f;

        float dist = delta.magnitude;
        float speed = dist / Mathf.Max(Time.deltaTime, 0.0001f);

        lastPos = currentPos;

        if (speed < minSpeed) return;

        accumDistance += dist;

        if (accumDistance >= stepDistance)
        {
            accumDistance = 0f;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
        audioSource.PlayOneShot(clip); // nutzt audioSource.volume
    }
}
