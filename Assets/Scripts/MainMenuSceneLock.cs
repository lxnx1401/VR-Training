using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuSceneLock : MonoBehaviour
{
    [Header("Freeze game time in MainMenu")]
    [SerializeField] private bool freezeTimeScale = true;

    [Header("Optional: disable locomotion input actions (recommended for XR)")]
    [Tooltip("Hier trägst du z.B. Move/Turn Actions ein (XRI Default Input Actions: Move/Turn).")]
    [SerializeField] private InputActionReference[] actionsToDisable;

    private float previousTimeScale = 1f;

    private void OnEnable()
    {
        previousTimeScale = Time.timeScale;

        if (freezeTimeScale)
            Time.timeScale = 0f;

        SetActionsEnabled(false);
    }

    private void OnDisable()
    {
        if (freezeTimeScale)
            Time.timeScale = previousTimeScale;

        SetActionsEnabled(true);
    }

    private void SetActionsEnabled(bool enabled)
    {
        if (actionsToDisable == null) return;

        for (int i = 0; i < actionsToDisable.Length; i++)
        {
            var a = actionsToDisable[i];
            if (a == null || a.action == null) continue;

            if (enabled) a.action.Enable();
            else a.action.Disable();
        }
    }
}
