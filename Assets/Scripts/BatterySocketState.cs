using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BatterySocketState : MonoBehaviour
{
    public bool IsBatteryInSocket { get; private set; }

    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    private void Awake()
    {
        if (!socket) socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnEntered);
        socket.selectExited.AddListener(OnExited);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnEntered);
        socket.selectExited.RemoveListener(OnExited);
    }

    private void OnEntered(SelectEnterEventArgs args) => IsBatteryInSocket = true;
    private void OnExited(SelectExitEventArgs args) => IsBatteryInSocket = false;
}
