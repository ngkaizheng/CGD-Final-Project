// using UnityEngine;
// using Fusion;
// using Unity.Cinemachine;
// using Fusion.Addons.KCC;

// public class SimpleCameraFollow : NetworkBehaviour
// {
//     [Header("Target Settings")]
//     public Transform player;  // Assign your player's transform in Inspector
//     public Transform CameraPivot; // Assign the head position GameObject
//     public Transform CameraHandle; // Assign the camera handle GameObject
//     private Player playerComponent;

//     [Header("Camera Settings")]
//     public Vector2 pitchClamp = new Vector2(-35f, 80f);
//     public float rotationSharpness = 10f;
//     public CinemachineCamera vcam;

//     [Networked] public Vector2 NetworkedLookRotation { get; set; }
//     [Networked] public Vector3 NetworkedCameraForward { get; set; }
//     [Networked] public Vector3 NetworkedCameraRight { get; set; }
//     [Networked] public Vector2 lockedLookRotation { get; set; }
//     [Networked] private NetworkButtons PreviousButtons { get; set; }

//     public void SetupLocalCamera(bool HasInputAuthority)
//     {
//         // vcam = FindAnyObjectByType<CinemachineCamera>();
//         if (vcam == null) Debug.LogWarning("Cinemachine Virtual Camera not found");
//         // vcam.enabled = true;
//         vcam.transform.gameObject.SetActive(HasInputAuthority); // Ensure the camera is active

//         vcam.Follow = CameraPivot.transform; // Use headposition for follow
//         vcam.LookAt = CameraPivot.transform; // Use headposition for look at
//     }

//     private void Start()
//     {
//         if (player == null)
//         {
//             Debug.LogError("Player not assigned to camera follow!");
//             return;
//         }

//         playerComponent = player.GetComponent<Player>();
//         if (playerComponent == null)
//         {
//             Debug.LogError("Player component not found on player object!");
//             return;
//         }

//         SetupLocalCamera(HasInputAuthority);
//     }


//     public override void FixedUpdateNetwork()
//     {
//         if (playerComponent == null)
//             return;
//         if (GetInput(out NetInput input))
//         {
//             if (!playerComponent.Object.IsValid) return;

//             if (HasStateAuthority)
//             {
//                 // Handle camera rotation based on input
//                 NetworkedLookRotation = KCCUtility.GetClampedEulerLookRotation(
//                     NetworkedLookRotation,
//                     input.LookDelta * playerComponent.NetworkedLookSensitivity,
//                     pitchClamp.x,
//                     pitchClamp.y
//                 );

//                 // Calculate camera vectors based on networked rotation
//                 Quaternion camRotation = Quaternion.Euler(NetworkedLookRotation);
//                 NetworkedCameraForward = camRotation * Vector3.forward;
//                 NetworkedCameraRight = camRotation * Vector3.right;
//             }

//             // if (HasStateAuthority)
//             // {
//             //     // When Alt is first pressed, lock the look rotation
//             //     if (input.Buttons.WasPressed(PreviousButtons, InputButton.Alt))
//             //     {
//             //         lockedLookRotation = NetworkedLookRotation;
//             //     }

//             //     // Always allow camera to look around
//             //     NetworkedLookRotation = KCCUtility.GetClampedEulerLookRotation(
//             //         NetworkedLookRotation,
//             //         input.LookDelta * playerComponent.NetworkedLookSensitivity,
//             //         pitchClamp.x,
//             //         pitchClamp.y
//             //     );

//             //     // Calculate camera vectors based on networked rotation
//             //     Quaternion camRotation = Quaternion.Euler(NetworkedLookRotation);
//             //     NetworkedCameraForward = camRotation * Vector3.forward;
//             //     NetworkedCameraRight = camRotation * Vector3.right;

//             //     // If Alt was just released, snap camera/player to lockedLookRotation
//             //     if (input.Buttons.WasReleased(PreviousButtons, InputButton.Alt))
//             //     {
//             //         NetworkedLookRotation = lockedLookRotation;
//             //     }
//             //     PreviousButtons = input.Buttons;
//             // }
//         }
//     }

//     public override void Render()
//     {
//         if (player == null) return;
//         if (Object == null || !Object.IsValid) return;

//         // Match player position exactly (no offset)
//         transform.position = player.position;

//         // Apply networked rotation to the holder (pivot) for camera rotation
//         Quaternion targetRotation = Quaternion.Euler(NetworkedLookRotation.x, NetworkedLookRotation.y, 0);

//         if (rotationSharpness > 0)
//         {
//             CameraPivot.transform.rotation = Quaternion.Slerp(
//                 CameraPivot.transform.rotation,
//                 targetRotation,
//                 rotationSharpness * Time.deltaTime
//             );
//         }
//         else
//         {
//             CameraPivot.transform.rotation = targetRotation;
//         }
//     }

//     public Vector3 GetCameraDirection(Vector2 inputDirection)
//     {
//         // Use networked vectors for consistent results across all clients
//         Vector3 forward = NetworkedCameraForward;
//         Vector3 right = NetworkedCameraRight;
//         return (forward * inputDirection.y + right * inputDirection.x).normalized;
//     }

//     public Vector3 GetCameraDirection(Vector2 inputDirection, bool useLocked = false)
//     {
//         Vector2 rotation = useLocked ? lockedLookRotation : NetworkedLookRotation;
//         Quaternion rot = Quaternion.Euler(0, rotation.y, 0);
//         Vector3 forward = rot * Vector3.forward;
//         Vector3 right = rot * Vector3.right;
//         return (forward * inputDirection.y + right * inputDirection.x).normalized;
//     }
// }