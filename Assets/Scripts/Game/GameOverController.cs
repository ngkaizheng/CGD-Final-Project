using UnityEngine;
using Fusion;
using System.Collections;

public class GameOverController : NetworkBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float gameOverDelay = 3f;

    [Header("Events")]
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

    public void CheckGameOverCondition()
    {
        if (!Runner.IsServer) return;

        if (!PlayerTracker.Instance.IsAnyPlayerAlive())
        {
            Debug.Log("Game Over condition met!");
            StartCoroutine(GameOverWithDelay());
        }
    }

    private IEnumerator GameOverWithDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);
        RPC_TriggerGameOver();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TriggerGameOver()
    {
        Debug.Log("Game Over - No living players remain!");
        gameOverEvent.Raise();
        EndGameUI.Instance.ShowGameOver();

        // Check role and grant appropriate achievement
        if (PlayerTracker.Instance.IsPlayerPontianak(Runner.LocalPlayer))
        {
            AchievementController.Instance.OnFirstPontianakPlayed.Raise();
            if (PontianakObjectiveController.Instance.GetKillCount() >= 1)
            {
                AchievementController.Instance.OnFirstPontianakHunt.Raise();
            }
        }
        // else
        // {
        //     AchievementController.Instance.OnFirstOutsiderPlayed.Raise();
        // }
    }
}