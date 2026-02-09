using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InSocketClickSetsBoolTrue_NoGrab : MonoBehaviour
{
    [Header("Socket state (Script liegt am Socket)")]
    [SerializeField] private BatterySocketState socketState;

    [Header("Input für PC-Test (Left Mouse)")]
    [SerializeField] private InputActionReference clickAction; // <Mouse>/leftButton

    [Header("Animator Target")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string boolName = "Pressed";

    [Header("Grab Interactable (muss AN bleiben für Socket!)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private void Awake()
    {
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnEnable()
    {
        // Wichtig: Grab bleibt enabled, sonst kann Socket nicht selecten!
        if (grab != null)
            grab.selectEntered.AddListener(OnGrabSelectEntered);

        if (clickAction != null)
        {
            clickAction.action.Enable();
            clickAction.action.performed += OnClick;
        }
    }

    private void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(OnGrabSelectEntered);

        if (clickAction != null)
            clickAction.action.performed -= OnClick;
    }

    // Wird aufgerufen, wenn irgendwer die Batterie selektiert (Socket ODER Hand)
    private void OnGrabSelectEntered(SelectEnterEventArgs args)
    {
        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        // Wenn der Socket selektiert (beim Einsetzen): erlauben!
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
            return;

        // Wenn die Hand/Interactor selektiert obwohl Batterie im Socket ist: sofort abbrechen
        if (grab.interactionManager != null)
            grab.interactionManager.SelectExit(args.interactorObject, grab);
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        if (socketState == null || !socketState.IsBatteryInSocket)
            return;

        if (targetAnimator == null || string.IsNullOrEmpty(boolName))
            return;

        targetAnimator.SetBool(boolName, true);
    }
}
