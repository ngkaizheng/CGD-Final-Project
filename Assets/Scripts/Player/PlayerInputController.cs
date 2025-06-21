using System;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private Player _player;
    private KCC kcc;

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    public void Initialize(Player player)
    {
        _player = player;
    }

    private void Awake()
    {
        kcc = GetComponent<KCC>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            if (!_player.isAlive()) return;

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
        //Since player need to hold to interact, use set instead of was pressed
        if (input.Buttons.IsSet(InputButton.E) && kcc.FixedData.IsGrounded)
        {
            // _player.Interact();
            Debug.Log("Interact pressed");
        }
        else if (input.Buttons.WasReleased(previousButtons, InputButton.E))
        {
            // _player.Interact();
            Debug.Log("Interact released");
        }
    }
}