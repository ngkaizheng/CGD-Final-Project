using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LobbyManager Instance { get; private set; }

    [Networked, Capacity(4), OnChangedRender(nameof(OnPlayersChanged))]
    public NetworkLinkedList<LobbyPlayerData> Players { get; } = default;

    [SerializeField] private NetworkObject _lobbyPlayerPrefab;
    [SerializeField] private LobbyPlayerListDataEvent _onPlayerListChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        OnPlayersChanged();
    }

    private void SpawnPlayerData(PlayerRef player)
    {
        var playerObj = Runner.Spawn(_lobbyPlayerPrefab, position: Vector3.zero, inputAuthority: player);
        playerObj.transform.SetParent(transform, false);
        playerObj.name = "LobbyPlayer_" + Players.Count + 1;

        Players.Add(playerObj.GetComponent<LobbyPlayerData>());
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            SpawnPlayerData(player);
            Debug.Log($"Player {player} joined the lobby. Total players: {Players.Count}");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            LobbyPlayerData playerData = null;
            foreach (var p in Players)
            {
                if (p.PlayerRef == player)
                {
                    playerData = p;
                    break;
                }
            }
            if (playerData != null)
            {
                Players.Remove(playerData);
                Runner.Despawn(playerData.Object);
            }
        }
    }

    public void StartGame()
    {
        if (Runner.IsServer && Players.Count > 0)
        {
            Debug.Log("Starting game with " + Players.Count + " players.");
        }
    }

    public void OnStartGameOrReadyClicked()
    {
        if (Runner.IsServer)
        {
            if (!CheckAllPlayersReady())
            {
                Debug.Log("Not all players are ready. Cannot start the game.");
                return;
            }
            StartGame();
        }
        else
        {
            TogglePlayerReady();
        }
    }

    public bool CheckAllPlayersReady()
    {
        if (!Runner.IsServer) return false;

        foreach (var player in Players)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    public void TogglePlayerReady()
    {
        LobbyPlayerData localPlayerData = null;
        foreach (var player in Players)
        {
            if (player.PlayerRef == Runner.LocalPlayer)
            {
                localPlayerData = player;
                break;
            }
        }
        if (localPlayerData != null)
        {
            localPlayerData.RPC_SetReady(!localPlayerData.IsReady);
        }
    }

    private void OnPlayersChanged()
    {
        _onPlayerListChanged.Raise(Players);
    }
}