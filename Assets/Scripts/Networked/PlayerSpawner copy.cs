// using UnityEngine;
// using Fusion;
// using System.Collections.Generic;

// public class PlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
// {
//     [SerializeField] private NetworkObject pontianakPrefab;
//     [SerializeField] private NetworkObject outsiderPrefab;

//     private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
//     private List<PlayerRef> joinOrder = new List<PlayerRef>();

//     public override void Spawned()
//     {
//     }

//     public void PlayerJoined(PlayerRef player)
//     {
//         if (Runner.IsServer)
//         {
//             joinOrder.Add(player);

//             SpawnPoint[] spawnPoints = Runner.SimulationUnityScene.GetComponents<SpawnPoint>(false);
//             Transform spawnPoint = null;
//             if (spawnPoints.Length > 0)
//                 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;

//             // // First player is Pontianak, others are Outsiders
//             // NetworkObject prefabToSpawn = (joinOrder[0] == player) ? pontianakPrefab : outsiderPrefab;
//             // SpawnPlayer(player, prefabToSpawn, spawnPoint);

//             // Debug.Log($"Player {player} joined as {(joinOrder[0] == player ? "Pontianak" : "Outsider")}. Total players: {_spawnedPlayers.Count}");
//             // Second player is Pontianak, others are Outsiders
//             NetworkObject prefabToSpawn = (joinOrder.Count == 2 && joinOrder[1] == player) ? pontianakPrefab : outsiderPrefab;
//             SpawnPlayer(player, prefabToSpawn, spawnPoint);

//             Debug.Log($"Player {player} joined as {((joinOrder.Count == 2 && joinOrder[1] == player) ? "Pontianak" : "Outsider")}. Total players: {_spawnedPlayers.Count}");
//         }
//     }

//     public void PlayerLeft(PlayerRef player)
//     {
//         if (Runner.IsServer && _spawnedPlayers.TryGetValue(player, out NetworkObject playerObj))
//         {
//             Runner.Despawn(playerObj);
//             _spawnedPlayers.Remove(player);
//             joinOrder.Remove(player);
//             Debug.Log($"Player {player} left the game. Remaining players: {_spawnedPlayers.Count}");
//         }
//     }

//     private void SpawnPlayer(PlayerRef playerRef, NetworkObject prefab, Transform spawnPoint = null)
//     {
//         if (!Runner.IsServer) return;

//         Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
//         Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

//         NetworkObject playerObj = Runner.Spawn(
//             prefab,
//             position: position,
//             rotation: rotation,
//             inputAuthority: playerRef
//         );
//         playerObj.name = $"PlayerObject_{playerRef}";
//         _spawnedPlayers[playerRef] = playerObj;

//         Runner.SetPlayerObject(playerRef, playerObj);
//     }

//     // public void PlayerJoined(PlayerRef player)
//     // {
//     //     if (Runner.IsServer)
//     //     {
//     //         SpawnPoint[] spawnPoints = Runner.SimulationUnityScene.GetComponents<SpawnPoint>(false);
//     //         if (spawnPoints.Length == 0)
//     //         {
//     //             SpawnPlayer(player);
//     //             return;
//     //         }
//     //         Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
//     //         SpawnPlayer(player, spawnPoint);
//     //         Debug.Log($"Player {player} joined the game. Total players: {_spawnedPlayers.Count}");
//     //     }
//     // }

//     // public void PlayerLeft(PlayerRef player)
//     // {
//     //     if (Runner.IsServer && _spawnedPlayers.TryGetValue(player, out NetworkObject playerObj))
//     //     {
//     //         Runner.Despawn(playerObj);
//     //         _spawnedPlayers.Remove(player);
//     //         Debug.Log($"Player {player} left the game. Remaining players: {_spawnedPlayers.Count}");
//     //     }
//     // }
//     // private void SpawnPlayer(PlayerRef playerRef, Transform spawnPoint = null)
//     // {
//     //     if (!Runner.IsServer) return;

//     //     Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
//     //     Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

//     //     NetworkObject playerObj = Runner.Spawn(
//     //         playerPrefab,
//     //         position: position,
//     //         rotation: rotation,
//     //         inputAuthority: playerRef
//     //     );
//     //     playerObj.name = $"PlayerObject_{playerRef}";
//     //     _spawnedPlayers[playerRef] = playerObj;

//     //     Runner.SetPlayerObject(playerRef, playerObj);
//     // }
// }