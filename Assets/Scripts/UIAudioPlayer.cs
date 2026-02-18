using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
    public static UIAudioPlayer I;

    [Header("Clips (only these)")]
    public AudioClip hover;
    public AudioClip click;

    [Header("Volume (optional)")]
    [Range(0f, 1f)] public float hoverVolume = 0.7f;
    [Range(0f, 1f)] public float clickVolume = 1.0f;

    AudioSource a;

    void Awake()
    {
        I = this;
        a = GetComponent<AudioSource>();
        if (!a) a = gameObject.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.spatialBlend = 0f; // 2D UI
    }

    public void PlayHover()
    {
        if (hover) a.PlayOneShot(hover, hoverVolume);
    }

    public void PlayClick()
    {
        if (click) a.PlayOneShot(click, clickVolume);
    }
}
