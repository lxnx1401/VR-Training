using UnityEngine;
using UnityEngine.UI;

public class GlobalAudioSettings : MonoBehaviour
{
    [Header("Global Volume (0..3)")]
    [Range(0f, 3f)]
    public float volume = 1f;

    [Tooltip("Optional: Slider im UI. Min=0, Max=3")]
    public Slider volumeSlider;

    [Tooltip("PlayerPrefs Key zum Speichern (optional)")]
    public string prefsKey = "GLOBAL_VOLUME";

    private void Awake()
    {
        // gespeicherten Wert laden, sonst Default
        volume = PlayerPrefs.GetFloat(prefsKey, volume);
        Apply(volume);

        // Slider initialisieren und verbinden
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 3f;
            volumeSlider.value = volume;

            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    public void OnSliderChanged(float v)
    {
        volume = v;
        Apply(volume);
        PlayerPrefs.SetFloat(prefsKey, volume);
        PlayerPrefs.Save();
    }

    public void Apply(float v)
    {
        // Global: beeinflusst alle AudioSources
        AudioListener.volume = Mathf.Clamp01(v / 3f); // 0..3 -> 0..1
    }

    // Optional: falls du Buttons statt Slider nutzt
    public void SetVolume(float v)
    {
        volume = v;
        Apply(volume);
        PlayerPrefs.SetFloat(prefsKey, volume);
        PlayerPrefs.Save();

        if (volumeSlider != null)
            volumeSlider.value = volume;
    }
}
