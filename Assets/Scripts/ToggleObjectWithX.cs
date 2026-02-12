using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleObjectWithX : MonoBehaviour
{
    [Header("Assign the object you want to toggle")]
    [SerializeField] private GameObject target;

    [Header("Input Action (set to X button on Left Hand)")]
    [SerializeField] private InputActionReference xButtonAction;

    private bool lastPressed;

    private void OnEnable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Disable();
    }

    private void Update()
    {
        if (target == null || xButtonAction == null) return;

        bool pressed = xButtonAction.action.ReadValue<float>() > 0.5f;

        // rising edge: only toggle once per press
        if (pressed && !lastPressed)
            target.SetActive(!target.activeSelf);

        lastPressed = pressed;
    }
}
