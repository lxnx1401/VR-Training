using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LayerSwitcher : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    // Hier legst du fest, welche Layer-Nummern genutzt werden
    public int defaultLayer = 0;   // Meistens "Default"
    public int highlightLayer = 6; // Die Nummer deines "Outline" Layers

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Wir abonnieren die Events vom XR Toolkit
        grabInteractable.hoverEntered.AddListener(SetHighlightLayer);
        grabInteractable.hoverExited.AddListener(SetDefaultLayer);
    }

    void OnDisable()
    {
        // Wichtig: Events wieder abmelden
        grabInteractable.hoverEntered.RemoveListener(SetHighlightLayer);
        grabInteractable.hoverExited.RemoveListener(SetDefaultLayer);
    }

    private void SetHighlightLayer(HoverEnterEventArgs args)
    {
        gameObject.layer = highlightLayer;
    }

    private void SetDefaultLayer(HoverExitEventArgs args)
    {
        gameObject.layer = defaultLayer;
    }
}
