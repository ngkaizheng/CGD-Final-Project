using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class InGamePlayerManager : NetworkBehaviour
{
    public static InGamePlayerManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private NetworkObject _playerDataPrefab;

    [Header("Event")]
    [SerializeField] private PlayerKillEvent playerKilledEvent;

    [Networked, Capacity(5)]
    public NetworkLinkedList<PlayerRef> pontianakDataDict { get; }
    [Networked, Capacity(5)]
    public NetworkLinkedList<PlayerRef> outsiderDataDict { get; }

    [Networked, Capacity(8), OnChangedRender(nameof(OnPlayerDataChanged))]
    public NetworkDictionary<PlayerRef, InGamePlayerData> playerDataDict { get; }

    private void Awake()
    {
        playerKilledEvent.OnRaised.AddListener(OnPlayerKilled);
    }

    private void OnDestroy()
    {
        playerKilledEvent.OnRaised.RemoveListener(OnPlayerKilled);
    }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;

        Debug.Log("InGamePlayerManager Spawned" + Runner.IsServer);
        if (Runner.IsServer)
        {
            Debug.Log("InGamePlayerManager Spawned on Server");
            InitializeAllPlayerData();
        }
    }
    private void InitializeAllPlayerData()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            Debug.Log($"Spawning player data for {player}");
            SpawnPlayerData(player);
        }
    }
    private void OnPlayerKilled(PlayerKillInfo info)
    {
        if (!Runner.IsServer) return;

        Debug.Log($"Player {info.Killer} killed {info.Victim}");
        // Find killer and victim InGamePlayerData
        var killerData = GetPlayerData(info.Killer);
        var victimData = GetPlayerData(info.Victim);

        Debug.Log($"Killer Data: {killerData}, Victim Data: {victimData}");

        if (killerData != null)
            killerData.RPC_AddKill();

        if (victimData != null)
            victimData.RPC_AddDeath();

        Debug.Log($"Updated Kills: {killerData?.Kills}, Deaths: {victimData?.Deaths}");
    }

    private void SpawnPlayerData(PlayerRef playerRef)
    {
        Runner.Spawn(
            _playerDataPrefab,
            inputAuthority: playerRef,
            onBeforeSpawned: (runner, obj) =>
            {
                var data = obj.GetComponent<InGamePlayerData>();
                playerDataDict.Set(playerRef, data);
            }
        );
    }

    private void OnPlayerDataChanged()
    {
        // LeaderboardUI.Instance.UpdateLeaderboard();
        //Store the player data  based on their roles in to dataDict
        pontianakDataDict.Clear();
        outsiderDataDict.Clear();
        foreach (var kvp in playerDataDict)
        {
            var playerData = kvp.Value;
            if (playerData == null) continue;
            if (playerData.Role == PlayerRole.PONTIANAK)
            {
                pontianakDataDict.Add(kvp.Key);
            }
            else if (playerData.Role == PlayerRole.OUTSIDER)
            {
                outsiderDataDict.Add(kvp.Key);
            }
        }
    }

    //Shared Mode RPC to set player data
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerData(PlayerRef playerRef, InGamePlayerData playerData)
    {
        if (!playerDataDict.ContainsKey(playerRef))
        {
            playerDataDict.Set(playerRef, playerData);
        }
    }

    public InGamePlayerData GetPlayerData(PlayerRef playerRef)
    => playerDataDict.TryGet(playerRef, out var data) ? data : null;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private List<InGamePlayerData> _debugPlayerDataList = new List<InGamePlayerData>();

    private void Update()
    {
        if (Object == null || !Object.IsValid) return;
        // Only update in Editor and in Play mode
        if (!Application.isPlaying) return;
        _debugPlayerDataList = playerDataDict.Select(kv => kv.Value).ToList();
    }
#endif


}