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

        // Check role and grant appropriate achievement
        if (PlayerTracker.Instance.IsPlayerPontianak(Runner.LocalPlayer))
        {
            var killCount = PontianakObjectiveController.Instance.GetKillCount();
            AchievementController.Instance.OnFirstPontianakPlayed.Raise();
            if (killCount >= 1)
            {
                AchievementController.Instance.OnFirstPontianakHunt.Raise();
                PlayFabLeaderboardController.Instance.UpdateKillsLeaderboard(killCount);
            }
            EndGameUI.Instance.ShowGameOver(PlayerRole.PONTIANAK, killCount);
        }
        else
        {
            EndGameUI.Instance.ShowGameOver(PlayerRole.OUTSIDER);
        }
        // else
        // {
        //     AchievementController.Instance.OnFirstOutsiderPlayed.Raise();
        // }
    }
}