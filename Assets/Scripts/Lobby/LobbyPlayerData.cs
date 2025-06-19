using System;
using Fusion;
using UnityEngine;

public class LobbyPlayerData : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public NetworkString<_32> Nickname { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public bool IsReady { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public PlayerRole Role { get; set; } = PlayerRole.OUTSIDER;
    [Networked, OnChangedRender(nameof(OnDataChanged))] public NetworkString<_32> SelectedSkinId { get; set; }


    [Header("Event Listening")]
    [SerializeField] private LobbyPlayerDataEvent _onPlayerDataUpdated;
    public bool isInitialized = false;

    public override void Spawned()
    {
        PlayerRef = Object.InputAuthority;

        if (Runner.IsServer)
        {
            // Initialize with default values
            Nickname = "...";
            if (Runner.IsServer && Runner.LocalPlayer == PlayerRef)
                IsReady = true;
            else
                IsReady = false;
        }

        isInitialized = true;

        if (Object.HasInputAuthority)
        {
            RPC_SetNickname(PlayFabUserController.Instance.DisplayName);
            var ownedSkins = PlayFabInventoryController.Instance.GetOwnedSkinsByRole(Role);
            if (ownedSkins != null && ownedSkins.Count > 0)
            {
                RPC_SetSelectedSkin(ownedSkins[0].itemId);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNickname(NetworkString<_32> nickname)
    {
        Nickname = nickname;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool isReady)
    {
        IsReady = isReady;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetSelectedSkin(string skinId)
    {
        SelectedSkinId = skinId;
    }

    public void RPC_KickPlayer()
    {
        if (Runner.IsServer)
        {
            Runner.Disconnect(PlayerRef); // Force disconnect
            // Runner.Despawn(Object); // Remove player object
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerRole(PlayerRole role)
    {
        Role = role;
    }

    private void OnDataChanged()
    {
        if (!isInitialized || string.IsNullOrEmpty(Nickname.ToString()))
            return;
        _onPlayerDataUpdated?.Raise(this);
    }
}