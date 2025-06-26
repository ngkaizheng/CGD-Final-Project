using UnityEngine;
using Fusion;
using System.Collections;

public class PontianakObjectiveController : NetworkBehaviour, IPlayerLeft
{
    public static PontianakObjectiveController Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float gameOverDelay = 3f;

    [Header("Events")]
    // [SerializeField] private GameEvent gameInitEvent;
    [SerializeField] private GameEvent gameOverEvent;

    // [Networked, Capacity(8)]
    // public NetworkLinkedList<PlayerRef> LivingPlayers { get; } = default;

    // private void Awake()
    // {
    //     gameInitEvent.OnRaised.AddListener(OnGameInit);
    // }

    // private void OnDestroy()
    // {
    //     gameInitEvent.OnRaised.RemoveListener(OnGameInit);
    // }

    // private void OnGameInit() //Only Server is executing this function
    // {
    //     if (Runner.IsServer)
    //     {
    //         InitializeLivingPlayers();
    //     }
    // }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;
    }

    // private void InitializeLivingPlayers()
    // {
    //     LivingPlayers.Clear();
    //     foreach (var playerRef in Runner.ActivePlayers)
    //     {
    //         var player = Runner.GetPlayerObject(playerRef)?.GetComponentInChildren<Outsider>();
    //         if (player != null && player.isAlive())
    //         {
    //             LivingPlayers.Add(playerRef);
    //         }
    //     }
    // }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        // LivingPlayers.Remove(playerRef);
        PlayerTracker.Instance.OnPlayerDied(playerRef);
        GameOverController.Instance.CheckGameOverCondition();
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        // LivingPlayers.Remove(playerRef);
        PlayerTracker.Instance.OnPlayerDied(playerRef);
        GameOverController.Instance.CheckGameOverCondition();
    }

    // private void CheckGameOverCondition()
    // {
    //     if (!Runner.IsServer) return;

    //     // If no living players remain, game over
    //     if (!PlayerTracker.Instance.IsAnyPlayerAlive())
    //     {
    //         Debug.Log("All outsiders have been eliminated!");
    //         StartCoroutine(GameOverWithDelay());
    //     }
    // }

    // private IEnumerator GameOverWithDelay()
    // {
    //     yield return new WaitForSeconds(gameOverDelay);
    //     RPC_TriggerGameOver();
    // }

    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_TriggerGameOver()
    // {
    //     gameOverEvent.Raise();
    //     Debug.Log("Game Over - Pontianak Team Wins!");
    // }

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
}