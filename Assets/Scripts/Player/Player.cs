using System;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float moveSpeed = 1f;
    public PlayerRole playerRole = PlayerRole.OUTSIDER;
    public GameObject playerModel;

    private Vector2 currentFacingRotation = Vector2.zero;
    private Vector2 targetFacingRotation = Vector2.zero;

    private KCC kcc;
    private PlayerInputController _inputController;
    [HideInInspector] public PlayerAction playerAction;
    [HideInInspector] public SimpleCameraFollow simpleCameraFollow;

    [Networked] public Vector2 _fixedLookRotation { get; set; }
    [Networked] public float NetworkedLookSensitivity { get; set; } = 0.1f;
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    // [Networked] public PlayerHealth Health { get; private set; }

    public virtual bool isAlive() => true;

    private void Awake()
    {
        kcc = GetComponent<KCC>();
        _inputController = GetComponent<PlayerInputController>();
        _inputController.Initialize(this);
        playerAction = GetComponent<PlayerAction>();
        simpleCameraFollow = transform.parent.GetComponentInChildren<SimpleCameraFollow>();
        playerModel = transform.GetChild(0).gameObject;
    }

    public override void Spawned()
    {
        // Health = GetComponent<PlayerHealth>();

        if (Object.InputAuthority == Runner.LocalPlayer)
        {
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
            // if (!isAlive()) return;

            bool useLocked = input.Buttons.IsSet(InputButton.Alt);
            if (!useLocked)
                _fixedLookRotation = KCCUtility.GetClampedEulerLookRotation(_fixedLookRotation, input.LookDelta * NetworkedLookSensitivity, -35f, 80f);

            UpdateRotation(input, useLocked);
            UpdateMoveDirection(input, useLocked);
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

    private void UpdateMoveDirection(NetInput input, bool useLocked)
    {
        if (!IsMoveable())
        {
            kcc.SetInputDirection(Vector3.zero);
            return;
        }
        Vector3 moveDir = simpleCameraFollow.GetCameraDirection(input.Direction, useLocked);
        moveDir *= moveSpeed;
        kcc.SetInputDirection(moveDir);
    }

    private void UpdateRotation(NetInput input, bool useLocked)
    {
        if (!IsMoveable()) return;
        // Check for Alt press in Player script
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Alt))
        {
            simpleCameraFollow.lockedLookRotation = _fixedLookRotation;
        }
        if (input.Buttons.WasReleased(PreviousButtons, InputButton.Alt))
        {
            simpleCameraFollow.NetworkedLookRotation = simpleCameraFollow.lockedLookRotation;
        }

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

    private bool IsMoveable()
    {
        if (playerAction.isInteracting)
            return false;

        return true;
    }

    #region Event Handlers
    private void HandleDeath(PlayerRef killer)
    {
        Debug.Log($"{Object.InputAuthority} has died, killed by {killer}");
        // Handle player death logic here, such as respawning or updating UI
    }
    #endregion


}
