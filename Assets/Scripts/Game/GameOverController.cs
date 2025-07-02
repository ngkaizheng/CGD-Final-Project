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
    [Networked] public bool IsGameOver { get; private set; }

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
            if (IsGameOver) return; // Prevent duplicate triggers
            IsGameOver = true;
            GameController.Instance.StopAllTimers();
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

        string reason = CheckEndGameReason();

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

            var accumulatedReward = (killCount * GameConfig.BASE_HUNT_REWARD) + GameConfig.BASE_PLAY_REWARD;
            PlayFabCurrencyController.Instance.GrantCurrency(accumulatedReward);

            EndGameUI.Instance.ShowGameOver(PlayerRole.PONTIANAK, killCount, accumulatedReward, reason: reason);
        }
        else
        {
            EndGameUI.Instance.ShowGameOver(PlayerRole.OUTSIDER, reason: reason);
        }
    }

    private string CheckEndGameReason()
    {
        int totalOutsiders = PlayerTracker.Instance.GetTotalOutsiders(Runner);
        int escapedOutsiders = PlayerTracker.Instance.EscapedPlayers.Count;
        int deadOutsiders = PlayerTracker.Instance.DeadPlayers.Count;
        string reason;
        if (GameController.Instance.IsTimeUp)
        {
            reason = "The cursed night has ended - ";
            if (escapedOutsiders > 0)
            {
                reason += $"{escapedOutsiders} villagers escaped to safety.";
            }
            else
            {
                reason += "no villagers survived the night.";
            }
        }
        else
        {
            if (escapedOutsiders == totalOutsiders)
            {
                reason = "All villagers have escaped! The Pontianak hunt nothing.";
            }
            else if (escapedOutsiders > 0)
            {
                reason = $"The night ends with {escapedOutsiders} survivors and {deadOutsiders} victims.";
            }
            else
            {
                reason = "A silent village... no villagers survived the Pontianak's wrath.";
            }
        }
        return reason;
    }
}
// else
// {
//     AchievementController.Instance.OnFirstOutsiderPlayed.Raise();
// }