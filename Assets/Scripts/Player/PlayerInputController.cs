using System;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : NetworkBehaviour
{
    private Player player;
    private KCC kcc;
    private Pontianak pontianak;

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    public void Initialize(Player player)
    {
        this.player = player;
    }

    private void Awake()
    {
        kcc = GetComponent<KCC>();
        player = GetComponent<Player>();
        TryGetComponent(out pontianak); // Will be null for non-Pontianak players
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            if (!player.isAlive()) return;

            // Debug.Log($"Processing input for player {Object.InputAuthority} at Tick: {Runner.Tick}, IsForward: {Runner.IsForward}, IsSimulation: {Runner.IsResimulation}");
            ProcessInput(input, PreviousButtons);
            PreviousButtons = input.Buttons;
        }
    }

    public override void Render()
    {
        // if (Object.IsProxy)
        // {
        //     ProcessButtons(Runner.GetInput<NetInput>(), PreviousButtons);
        // }
    }

    private void ProcessInput(NetInput input, NetworkButtons previousButtons)
    {
        // if (input.Buttons.IsSet(InputButton.LeftClick))
        // {
        //     _player.LeftClick();
        // }

        // if (input.Buttons.IsSet(InputButton.RightClick))
        // {
        //     _player.RightClick();
        // }

        // if (input.Buttons.WasPressed(previousButtons, InputButton.Reload))
        // {
        //     _player.Reload();
        // }

        // if (input.Buttons.WasPressed(previousButtons, InputButton.E))
        // {
        //     // _playerTexture.NextTexture();
        // }
        CheckJump(input, previousButtons);
        CheckInteract(input, previousButtons);
        CheckAbilities(input, previousButtons);
        CheckAttack(input, previousButtons);
    }

    private void CheckJump(NetInput input, NetworkButtons previousButtons)
    {
        if (input.Buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.FixedData.IsGrounded)
        {
            kcc.Jump(Vector3.up * 5f);
        }
    }

    private void CheckInteract(NetInput input, NetworkButtons previousButtons)
    {
        // Update the interact state based on button press/release
        bool isInteracting = input.Buttons.IsSet(InputButton.E) && kcc.FixedData.IsGrounded;
        player.playerAction.Interact(isInteracting);

        // //Since player need to hold to interact, use set instead of was pressed
        // if (input.Buttons.IsSet(InputButton.E) && kcc.FixedData.IsGrounded)
        // {
        //     player.playerAction.Interact(player.gameObject.transform, true);
        //     Debug.Log("Interact pressed");
        // }
        // else if (input.Buttons.WasReleased(previousButtons, InputButton.E))
        // {
        //     player.playerAction.Interact(player.gameObject.transform, false);
        //     Debug.Log("Interact released");
        // }
    }

    private void CheckAbilities(NetInput input, NetworkButtons previousButtons)
    {
        // Only check abilities if this is a Pontianak
        if (pontianak != null)
        {
            pontianak.CheckAbilityInput(input.Buttons, previousButtons);
        }
    }

    private void CheckAttack(NetInput input, NetworkButtons previousButtons)
    {
        // Check for left click attack
        if (pontianak == null) return;
        if (pontianak.isUsingAbility) return;

        if (input.Buttons.WasPressed(previousButtons, InputButton.LeftClick))
        {
            pontianak.playerAttack.PerformAttack();
        }
    }

    #region Local Input Handling
    private void Update()
    {
        HandleLocalInput();
    }
    public void HandleLocalInput()
    {
        // This method can be used to handle local input if needed
        // For example, you can check for keyboard input here
        // and call the appropriate methods on the player or pontianak.
        if (GameSettings.Instance != null)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                bool isActive = GameSettings.Instance.ToggleSettingsPanelBool();
                Debug.Log("Settings panel toggled: " + isActive);
                if (isActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Debug.Log("Cursor locked and hidden");
                }
            }
        }
    }
    #endregion
}
