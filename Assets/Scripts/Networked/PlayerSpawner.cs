using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkObject playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    public override void Spawned()
    {
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            SpawnPoint[] spawnPoints = Runner.SimulationUnityScene.GetComponents<SpawnPoint>(false);
            if (spawnPoints.Length == 0)
            {
                SpawnPlayer(player);
                return;
            }
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
            SpawnPlayer(player, spawnPoint);
            Debug.Log($"Player {player} joined the game. Total players: {_spawnedPlayers.Count}");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer && _spawnedPlayers.TryGetValue(player, out NetworkObject playerObj))
        {
            Runner.Despawn(playerObj);
            _spawnedPlayers.Remove(player);
            Debug.Log($"Player {player} left the game. Remaining players: {_spawnedPlayers.Count}");
        }
    }
    private void SpawnPlayer(PlayerRef playerRef, Transform spawnPoint = null)
    {
        if (!Runner.IsServer) return;

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        NetworkObject playerObj = Runner.Spawn(
            playerPrefab,
            position: position,
            rotation: rotation,
            inputAuthority: playerRef
        );
        playerObj.name = $"PlayerObject_{playerRef}";
        _spawnedPlayers[playerRef] = playerObj;

        Runner.SetPlayerObject(playerRef, playerObj);
    }
}