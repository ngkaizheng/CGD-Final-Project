using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LobbyManager Instance { get; private set; }

    [Networked, Capacity(4), OnChangedRender(nameof(OnPlayersChanged))]
    public NetworkLinkedList<LobbyPlayerData> Players { get; } = default;

    [SerializeField] private NetworkObject _lobbyPlayerPrefab;
    [SerializeField] private LobbyPlayerListDataEvent _onPlayerListChanged;

    public bool isTesting = false;

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
        // playerObj.transform.SetParent(transform, false);
        playerObj.name = "LobbyPlayer_" + Players.Count + 1;

        Players.Add(playerObj.GetComponent<LobbyPlayerData>());
    }

    #region PlayerJoined Left
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
    #endregion

    #region StartGame
    public void StartGame()
    {
        if (Runner.IsServer && Players.Count > 0)
        {
            Runner.SessionInfo.IsOpen = false; // Lock the session
            Debug.Log("Starting game with " + Players.Count + " players.");
            SceneRef gameScene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{GameConfig.GAME_SCENE}.unity"));
            Runner.LoadScene(gameScene, LoadSceneMode.Single);
        }
    }
    #endregion

    #region  OnStartGameOrReadyClicked
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
    #endregion

    #region CheckAllPlayersReady
    // public bool CheckAllPlayersReady()
    // {
    //     if (!Runner.IsServer) return false;

    //     foreach (var player in Players)
    //     {
    //         if (!player.IsReady)
    //         {
    //             return false;
    //         }
    //     }

    //     if (isTesting)
    //         return true;

    //     // Check role distribution
    //     int pontianakCount = 0;
    //     int outsiderCount = 0;

    //     foreach (var player in Players)
    //     {
    //         switch (player.Role)
    //         {
    //             case PlayerRole.PONTIANAK:
    //                 pontianakCount++;
    //                 break;
    //             case PlayerRole.OUTSIDER:
    //                 outsiderCount++;
    //                 break;
    //         }
    //     }

    //     if (pontianakCount != 1)
    //     {
    //         Debug.Log($"Need exactly one Pontianak to start (current: {pontianakCount}).");
    //         return false;
    //     }

    //     if (outsiderCount < 1)
    //     {
    //         Debug.Log($"Need at least one Outsider to start (current: {outsiderCount}).");
    //         return false;
    //     }
    //     return true;
    // }
    public bool CheckAllPlayersReady()
    {
        if (!Runner.IsServer) return false;
        if (isTesting) return true;

        var status = GetGameStartStatus();
        return status.allReady && status.pontianakCount == 1 && status.outsiderCount >= 1;
    }

    public (bool allReady, int pontianakCount, int outsiderCount, string statusMessage) GetGameStartStatus()
    {
        bool allReady = true;
        int pontianakCount = 0;
        int outsiderCount = 0;
        string statusMessage = "";

        foreach (var player in Players)
        {
            if (!player.IsReady) allReady = false;

            switch (player.Role)
            {
                case PlayerRole.PONTIANAK: pontianakCount++; break;
                case PlayerRole.OUTSIDER: outsiderCount++; break;
            }
        }

        if (!allReady)
        {
            statusMessage = "Waiting for all players to be ready...";
        }
        else if (pontianakCount != 1)
        {
            statusMessage = $"Need exactly 1 Pontianak (current: {pontianakCount})";
        }
        else if (outsiderCount < 1)
        {
            statusMessage = $"Need at least 1 Outsider (current: {outsiderCount})";
        }
        else
        {
            statusMessage = "Ready to start game!";
        }

        return (allReady, pontianakCount, outsiderCount, statusMessage);
    }
    #endregion

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