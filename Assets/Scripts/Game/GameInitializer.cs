using UnityEngine;
using Fusion;
using System.Collections;

public class GameInitializer : NetworkBehaviour
{
    [SerializeField] private PlayerSpawner _playerSpawner;

    public GameEvent gameInitEvent;

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            InitializeGame();
        }
    }

    private void InitializeGame()
    {
        // 1. Wait for all players to have their InGamePlayerData spawned
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (InGamePlayerManager.Instance == null)
        {
            yield return new WaitForSeconds(0.3f);
        }

        // Wait for all players to have data
        while (!AllPlayersHaveData())
        {
            yield return new WaitForSeconds(0.3f);
        }

        // 3. Spawn all players
        foreach (var player in Runner.ActivePlayers)
        {
            _playerSpawner.SpawnPlayer(player);
        }

        gameInitEvent.Raise();
    }

    private bool AllPlayersHaveData()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            if (InGamePlayerManager.Instance.GetPlayerData(player) == null)
                return false;
        }
        return true;
    }
}