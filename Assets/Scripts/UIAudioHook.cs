using UnityEngine;
using UnityEngine.EventSystems;

public class UIAudioHook : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public bool hoverSound = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound && UIAudioPlayer.I) UIAudioPlayer.I.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIAudioPlayer.I) UIAudioPlayer.I.PlayClick();
    }
}
