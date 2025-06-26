using UnityEngine;
using Fusion;
using System.Collections;

public class PontianakObjectiveController : NetworkBehaviour, IPlayerLeft
{
    public static PontianakObjectiveController Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float gameOverDelay = 3f;
    [Networked] public int killCount { get; set; } = 0;

    [Header("Events")]
    // [SerializeField] private GameEvent gameInitEvent;
    [SerializeField] private GameEvent gameOverEvent;

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        // LivingPlayers.Remove(playerRef);
        PlayerTracker.Instance.OnPlayerLeft(playerRef);
        GameOverController.Instance.CheckGameOverCondition();
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        // LivingPlayers.Remove(playerRef);
        PlayerTracker.Instance.OnPlayerDied(playerRef);
        GameOverController.Instance.CheckGameOverCondition();
        killCount++;
    }

    // For other systems to notify player death
    public void ReportPlayerDeath(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            OnPlayerDied(playerRef);
        }
        else
        {
            RPC_ReportPlayerDeath(playerRef);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReportPlayerDeath(PlayerRef playerRef)
    {
        OnPlayerDied(playerRef);
    }

    public int GetKillCount()
    {
        return killCount;
    }
}