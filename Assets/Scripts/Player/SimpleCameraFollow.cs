using UnityEngine;
using Fusion;
using Unity.Cinemachine;
using Fusion.Addons.KCC;

public class SimpleCameraFollow : NetworkBehaviour
{
    [Header("Target Settings")]
    public Transform player;  // Assign your player's transform in Inspector
    public Transform CameraPivot; // Assign the head position GameObject
    public Transform CameraHandle; // Assign the camera handle GameObject
    private Player playerComponent;

    [Header("Camera Settings")]
    public Vector2 pitchClamp = new Vector2(-35f, 80f);
    public float rotationSharpness = 10f;
    public CinemachineCamera vcam;

    [Networked] public Vector2 NetworkedLookRotation { get; set; }
    [Networked] public Vector3 NetworkedCameraForward { get; set; }
    [Networked] public Vector3 NetworkedCameraRight { get; set; }
    [Networked] public Vector2 lockedLookRotation { get; set; }
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    // Add these new variables for local prediction
    private Vector2 _localLookRotation;
    private bool _hasLocalInput;
    private Vector2 _lastReceivedNetworkedLookRotation;

    public void SetupLocalCamera(bool HasInputAuthority)
    {
        // vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam == null) Debug.LogWarning("Cinemachine Virtual Camera not found");
        // vcam.enabled = true;
        vcam.transform.gameObject.SetActive(HasInputAuthority); // Ensure the camera is active

        vcam.Follow = CameraPivot.transform; // Use headposition for follow
        vcam.LookAt = CameraPivot.transform; // Use headposition for look at
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned to camera follow!");
            return;
        }

        playerComponent = player.GetComponent<Player>();
        if (playerComponent == null)
        {
            Debug.LogError("Player component not found on player object!");
            return;
        }

        SetupLocalCamera(HasInputAuthority);
    }


    public override void FixedUpdateNetwork()
    {
        if (playerComponent == null)
            return;
        if (GetInput(out NetInput input))
        {
            if (!playerComponent.Object.IsValid) return;

            // Store whether we have local input this tick
            _hasLocalInput = input.LookDelta.sqrMagnitude > 0.001f;

            if (HasStateAuthority)
            {
                // Handle camera rotation based on input
                NetworkedLookRotation = KCCUtility.GetClampedEulerLookRotation(
                    NetworkedLookRotation,
                    input.LookDelta * playerComponent.NetworkedLookSensitivity,
                    pitchClamp.x,
                    pitchClamp.y
                );
            }

            // LOCAL player does additional prediction
            if (HasInputAuthority)
            {
                _localLookRotation = KCCUtility.GetClampedEulerLookRotation(
                    _localLookRotation,
                    input.LookDelta * playerComponent.NetworkedLookSensitivity,
                    pitchClamp.x,
                    pitchClamp.y
                );
            }

            // Calculate camera vectors based on appropriate rotation
            Quaternion camRotation = Quaternion.Euler(HasInputAuthority ? _localLookRotation : NetworkedLookRotation);
            NetworkedCameraForward = camRotation * Vector3.forward;
            NetworkedCameraRight = camRotation * Vector3.right;
        }
    }

    public override void Render()
    {
        if (player == null) return;
        if (Object == null || !Object.IsValid) return;

        // Match player position exactly (no offset)
        transform.position = player.position;

        // // Apply networked rotation to the holder (pivot) for camera rotation
        // Quaternion targetRotation = Quaternion.Euler(NetworkedLookRotation.x, NetworkedLookRotation.y, 0);

        // if (rotationSharpness > 0)
        // {
        //     CameraPivot.transform.rotation = Quaternion.Slerp(
        //         CameraPivot.transform.rotation,
        //         targetRotation,
        //         rotationSharpness * Time.deltaTime
        //     );
        // }
        // else
        // {
        //     CameraPivot.transform.rotation = targetRotation;
        // }

        // For local player with input authority, use the locally predicted rotation
        Vector2 renderRotation = HasInputAuthority ? _localLookRotation : NetworkedLookRotation;

        // Apply rotation to the holder (pivot) for camera rotation
        Quaternion targetRotation = Quaternion.Euler(renderRotation.x, renderRotation.y, 0);

        if (rotationSharpness > 0)
        {
            CameraPivot.transform.rotation = Quaternion.Slerp(
                CameraPivot.transform.rotation,
                targetRotation,
                rotationSharpness * Time.deltaTime
            );
        }
        else
        {
            CameraPivot.transform.rotation = targetRotation;
        }

        // Handle reconciliation if we're the local player
        if (HasInputAuthority && !_hasLocalInput)
        {
            // Smoothly reconcile if there's no local input
            _localLookRotation = Vector2.Lerp(_localLookRotation, NetworkedLookRotation, Runner.DeltaTime * 10f);
        }
        else if (HasInputAuthority && NetworkedLookRotation != _lastReceivedNetworkedLookRotation)
        {
            // New networked data arrived - reconcile
            _lastReceivedNetworkedLookRotation = NetworkedLookRotation;

            // Only reconcile if the difference is significant
            if (Vector2.Distance(_localLookRotation, NetworkedLookRotation) > 1f)
            {
                _localLookRotation = NetworkedLookRotation;
            }
        }
    }

    public Vector3 GetCameraDirection(Vector2 inputDirection)
    {
        // Use networked vectors for consistent results across all clients
        Vector3 forward = NetworkedCameraForward;
        Vector3 right = NetworkedCameraRight;
        return (forward * inputDirection.y + right * inputDirection.x).normalized;
    }

    public Vector3 GetCameraDirection(Vector2 inputDirection, bool useLocked = false)
    {
        Vector2 rotation = useLocked ? lockedLookRotation : NetworkedLookRotation;
        Quaternion rot = Quaternion.Euler(0, rotation.y, 0);
        Vector3 forward = rot * Vector3.forward;
        Vector3 right = rot * Vector3.right;
        return (forward * inputDirection.y + right * inputDirection.x).normalized;
    }
}