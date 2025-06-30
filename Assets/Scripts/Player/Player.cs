using System;
using System.Linq;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Player : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    public PlayerRole playerRole = PlayerRole.OUTSIDER;

    [Header("Model References")]
    [SerializeField] private Transform modelParent;
    public GameObject playerModel;

    private Vector2 currentFacingRotation = Vector2.zero;
    private Vector2 targetFacingRotation = Vector2.zero;

    public KCC kcc;
    private PlayerInputController _inputController;
    [HideInInspector] public PlayerAction playerAction;
    [HideInInspector] public SimpleCameraFollow simpleCameraFollow;
    [HideInInspector] public SimpleAnimator simpleAnimator;

    [Networked] public Vector2 _fixedLookRotation { get; set; }
    [Networked] public float NetworkedLookSensitivity { get; set; } = 0.1f;
    [Networked] public bool IsCrouching { get; set; }
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    // [Networked] public PlayerHealth Health { get; private set; }

    public virtual bool isAlive() => true;

    protected virtual void Awake()
    {
        kcc = GetComponent<KCC>();
        _inputController = GetComponent<PlayerInputController>();
        _inputController.Initialize(this);
        playerAction = GetComponent<PlayerAction>();
        simpleCameraFollow = transform.parent.GetComponentInChildren<SimpleCameraFollow>();
        simpleAnimator = GetComponent<SimpleAnimator>();
        playerModel = modelParent.GetChild(0).gameObject;
        // playerModel.SetActive(false); // Initially disable the model
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

        // // Change skin based on LobbyPlayerData
        // if (Object.HasStateAuthority)
        // {
        //     // Get skin ID from lobby data
        //     var lobbyData = FindObjectsByType<LobbyPlayerData>(FindObjectsSortMode.None).FirstOrDefault(p => p.PlayerRef == Object.InputAuthority);
        //     if (lobbyData != null)
        //     {
        //         SpawnModel(lobbyData.SelectedSkinId.ToString());
        //     }
        //     else
        //     {
        //         playerModel.SetActive(true);
        //     }
        // }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            if (NetworkedLookSensitivity != GameConfig.LOOK_SENSITIVITY)
            {
                RPC_UpdateLookSensitivity(GameConfig.LOOK_SENSITIVITY);
            }
        }

        if (GetInput(out NetInput input))
        {
            // if (!isAlive()) return;

            bool useLocked = input.Buttons.IsSet(InputButton.Alt);
            if (!useLocked)
                _fixedLookRotation = KCCUtility.GetClampedEulerLookRotation(_fixedLookRotation, input.LookDelta * NetworkedLookSensitivity, -35f, 80f);

            UpdateCrouch(input, useLocked);
            UpdateRotation(input, useLocked);
            UpdateMoveDirection(input, useLocked);
            PreviousButtons = input.Buttons;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

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
#endif

    private void UpdateCrouch(NetInput input, bool useLocked)
    {
        if (!IsMoveable() || playerRole == PlayerRole.PONTIANAK) return;
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Crouch))
        {
            IsCrouching = !IsCrouching;
            simpleAnimator.SetCrouching(IsCrouching);
        }
    }

    private void UpdateMoveDirection(NetInput input, bool useLocked)
    {
        if (!IsMoveable())
        {
            kcc.SetInputDirection(Vector3.zero);
            return;
        }
        float speedMultiplier = IsCrouching ? crouchSpeedMultiplier : 1f;

        Vector3 moveDir = simpleCameraFollow.GetCameraDirection(input.Direction, useLocked);
        moveDir *= moveSpeed * speedMultiplier;
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

    protected virtual bool IsMoveable()
    {
        if (playerAction.isInteracting)
            return false;

        return true;
    }

    #region Look Sensitivity
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateLookSensitivity(float sensitivity)
    {
        if (HasStateAuthority)
        {
            NetworkedLookSensitivity = sensitivity;
            Debug.Log($"[{Object.Id}] Server set NetworkedLookSensitivity: {NetworkedLookSensitivity}");
        }
    }
    #endregion

    // Disable collider and KCC when player is dead
    public void HandleDeath()
    {
        kcc.enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Escape()
    {
        HandleEscape();
    }

    public void HandleEscape()
    {
        HandleDeath();
        //Disable model
        if (playerModel != null)
        {
            playerModel.SetActive(false);
        }
    }

    #region Model Management
    // private void SpawnModel(string skinId)
    // {
    //     // Get skin data from inventory
    //     var skinData = PlayFabInventoryController.Instance.GetSkinData(skinId);
    //     if (skinData == null)
    //     {
    //         Debug.LogWarning($"No skin data found for ID: {skinId}");
    //         return;
    //     }
    //     playerModel.SetActive(false); // Disable existing model if any
    //     // Spawn the correct model directly
    //     var spawnedNetworkObject = Runner.Spawn(
    //         skinData.modelPrefab,
    //         position: Vector3.zero,
    //         rotation: Quaternion.identity,
    //         inputAuthority: Object.InputAuthority,
    //         onBeforeSpawned: (runner, networkObject) =>
    //         {
    //             networkObject.transform.SetParent(modelParent, true);
    //         }
    //     );
    //     var playerAnimRelay = spawnedNetworkObject.GetComponent<PlayerAnimRelay>();
    //     if (playerAnimRelay != null)
    //         playerAnimRelay.Init();
    //     playerModel = spawnedNetworkObject.gameObject;
    //     playerModel.SetActive(true); // Enable the new model

    // }
    #endregion

}
