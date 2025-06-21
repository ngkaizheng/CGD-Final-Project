using System;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _moveSpeed = 10f;

    [Header("Camera Settings")]
    [SerializeField] private Transform camLookAt;
    [SerializeField] private Transform followTarget;


    [Networked] public PlayerHealth Health { get; private set; }

    // private NetworkCharacterController _cc;
    // private CharacterController _characterController;
    private KCC kcc;
    private PlayerInputController _inputController;

    private Vector2 currentFacingRotation;
    private Vector2 targetFacingRotation;
    public float rotationSpeed = 20f;
    [Networked] public Vector2 _fixedLookRotation { get; set; }
    [Networked] public float NetworkedLookSensitivity { get; set; } = 0.1f;
    public SimpleCameraFollow simpleCameraFollow; // Reference to SimpleCameraFollow script
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    private void Awake()
    {
        // _cc = GetComponent<NetworkCharacterController>();
        // _characterController = GetComponent<CharacterController>();
        kcc = GetComponent<KCC>();
        _inputController = GetComponent<PlayerInputController>();


        _inputController.Initialize(this);
    }

    public override void Spawned()
    {
        Health = GetComponent<PlayerHealth>();

        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            // CameraController.Instance.SetFollowTarget(camLookAt, followTarget);  

            InputManager inputManager = GetComponentInChildren<InputManager>();
            if (inputManager != null)
            {
                var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
                if (runner.IsRunning)
                {
                    runner.AddGlobal(inputManager);
                    runner.AddCallbacks(inputManager);
                }
                inputManager.SetLocalPlayer(this);
                Debug.Log("Local player set in InputManager");
            }
            else
            {
                Debug.LogWarning("InputManager not found in the scene.");
            }

        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            if (!isAlive()) return;

            if (!input.Buttons.IsSet(InputButton.Alt))
                _fixedLookRotation = KCCUtility.GetClampedEulerLookRotation(_fixedLookRotation, input.LookDelta * NetworkedLookSensitivity, -35f, 80f);

            // Use WasReleased for Alt
            // if (input.Buttons.WasReleased(PreviousButtons, InputButton.Alt))
            // {
            //     Debug.Log($"Player {Object.InputAuthority} released Alt, setting lockedLookRotation to {_fixedLookRotation}");
            //     _fixedLookRotation = simpleCameraFollow.lockedLookRotation;
            //     currentFacingRotation = simpleCameraFollow.lockedLookRotation;
            //     targetFacingRotation = simpleCameraFollow.lockedLookRotation;
            //     kcc.SetLookRotation(currentFacingRotation);
            // }

            UpdateRotation(input);
            UpdateMoveDirection(input);
            // if (data.Buttons.IsSet(InputButton.Jump))
            // {
            //     _cc.Jump();
            // }
            PreviousButtons = input.Buttons;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw currentFacingRotation direction in the Scene view
        Vector3 forward = Quaternion.Euler(currentFacingRotation.x, currentFacingRotation.y, 0) * Vector3.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, forward * 5f);

        // Draw _fixedLookRotation
        Vector3 fixedLookForward = Quaternion.Euler(_fixedLookRotation.x, _fixedLookRotation.y, 0) * Vector3.forward;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fixedLookForward * 5f);

        //Draw simpleCameraFollow.lockedLookRotation
        Vector3 lockedLookForward = Quaternion.Euler(simpleCameraFollow.lockedLookRotation.x, simpleCameraFollow.lockedLookRotation.y, 0) * Vector3.forward;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, lockedLookForward * 5f);
    }

    private void UpdateMoveDirection(NetInput input)
    {
        bool useLocked = input.Buttons.IsSet(InputButton.Alt);
        // Vector2 move2D = input.Direction.normalized;
        // Vector3 move = new Vector3(move2D.x, 0, move2D.y); // XZ plane
        // Vector3 inputDirection = kcc.Data.TransformRotation * move;

        // Vector3 moveDir = simpleCameraFollow.GetCameraDirection(input.Direction);
        // _cc.Move(move);

        Vector3 moveDir = simpleCameraFollow.GetCameraDirection(input.Direction, useLocked);
        kcc.SetInputDirection(moveDir);
        Debug.Log($"Player {Object.InputAuthority} moving with direction: {moveDir} {useLocked}");
    }

    // private void UpdateRotation(NetInput input)
    // {
    //     Vector3 moveDirection = input.Direction.X0Y();
    //     if (moveDirection.IsZero() == false)
    //     {
    //         Vector3 inputDirection = Quaternion.Euler(0.0f, _fixedLookRotation.y, 0.0f) * moveDirection;
    //         targetFacingRotation = new Vector2(_fixedLookRotation.x, KCCUtility.GetClampedEulerLookRotation(inputDirection.OnlyXZ().normalized).y);
    //     }
    //     else
    //     {
    //         targetFacingRotation = new Vector2(currentFacingRotation.x, currentFacingRotation.y);
    //     }

    //     Quaternion currentRotation = Quaternion.Euler(currentFacingRotation.x, currentFacingRotation.y, 0);
    //     Quaternion targetRotation = Quaternion.Euler(targetFacingRotation.x, targetFacingRotation.y, 0);
    //     Quaternion smoothRotation = Quaternion.Slerp(currentRotation, targetRotation, Runner.DeltaTime * rotationSpeed);
    //     currentFacingRotation = new Vector2(smoothRotation.eulerAngles.x, smoothRotation.eulerAngles.y);

    //     kcc.SetLookRotation(currentFacingRotation);
    // }
    private void UpdateRotation(NetInput input)
    {
        // Check for Alt press in Player script
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Alt))
        {
            simpleCameraFollow.lockedLookRotation = _fixedLookRotation;
        }
        if (input.Buttons.WasReleased(PreviousButtons, InputButton.Alt))
        {
            simpleCameraFollow.NetworkedLookRotation = simpleCameraFollow.lockedLookRotation;
        }

        bool useLocked = input.Buttons.IsSet(InputButton.Alt);
        Vector2 lookRotation = useLocked ? simpleCameraFollow.lockedLookRotation : _fixedLookRotation;

        Vector3 moveDirection = input.Direction.X0Y();
        if (moveDirection.IsZero() == false)
        {
            Vector3 inputDirection = Quaternion.Euler(0.0f, lookRotation.y, 0.0f) * moveDirection;
            targetFacingRotation = new Vector2(lookRotation.x, KCCUtility.GetClampedEulerLookRotation(inputDirection.OnlyXZ().normalized).y);
        }
        else
        {
            targetFacingRotation = new Vector2(currentFacingRotation.x, currentFacingRotation.y);
        }

        Quaternion currentRotation = Quaternion.Euler(currentFacingRotation.x, currentFacingRotation.y, 0);
        Quaternion targetRotation = Quaternion.Euler(targetFacingRotation.x, targetFacingRotation.y, 0);
        Quaternion smoothRotation = Quaternion.Slerp(currentRotation, targetRotation, Runner.DeltaTime * rotationSpeed);
        currentFacingRotation = new Vector2(smoothRotation.eulerAngles.x, smoothRotation.eulerAngles.y);

        kcc.SetLookRotation(currentFacingRotation);
    }

    // private void UpdateRotation(NetInput input)
    // {
    //     Vector3 lookDirection = new Vector3(
    //         input.LookDirection.x,
    //         0,
    //         input.LookDirection.y
    //     ).normalized;
    //     if (lookDirection.sqrMagnitude > 0.001f)
    //     {
    //         Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
    //         Quaternion smoothedRotation = Quaternion.Slerp(
    //             transform.rotation,
    //             targetRotation,
    //             _rotationSpeed * Runner.DeltaTime
    //         );
    //         transform.rotation = smoothedRotation;
    //     }

    //     if (Object.HasStateAuthority)
    //     {
    //         Debug.DrawLine(
    //             transform.position,
    //             transform.position + lookDirection * 2f,
    //             Color.red,
    //             1f
    //         );
    //     }
    //     else
    //     {
    //         Debug.DrawLine(
    //             transform.position,
    //             transform.position + lookDirection * 2f,
    //             Color.blue,
    //             1f
    //         );
    //     }
    // }

    #region Player Actions
    public void LeftClick()
    {
        Debug.Log("Left Click Action Triggered");
        // Implement left click action logic here
    }
    public void RightClick()
    {
        Debug.Log("Right Click Action Triggered");
        // Implement right click action logic here
        // Deduct 10 HP from the player
        if (Health != null && Health.IsAlive)
        {
            Health.TakeDamage(10, Object.InputAuthority);
        }

    }
    public void Reload()
    {
        Debug.Log("Reload Action Triggered");
        // Implement reload action logic here
    }
    #endregion

    #region Event Handlers
    private void HandleDeath(PlayerRef killer)
    {
        Debug.Log($"{Object.InputAuthority} has died, killed by {killer}");
        // Handle player death logic here, such as respawning or updating UI
    }
    #endregion

    public bool isAlive()
    {
        return Health != null && Health.IsAlive;
    }
}