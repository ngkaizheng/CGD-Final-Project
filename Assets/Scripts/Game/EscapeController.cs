using UnityEngine;
using Fusion;
using System.Collections;

public class EscapeController : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private int currencyReward = 50;
    [SerializeField] private float gameOverDelay = 3f;

    [Header("Events")]
    [SerializeField] private GameEvent objectiveCompleteEvent;
    [SerializeField] private GameEvent playerEscapedEvent;
    [SerializeField] private GameEvent gameOverEvent;

    [Header("References")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private EscapeDoor escapeDoor;


    public static EscapeController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        escapeDoor.SetDoorActive(false);
        if (visualEffect != null)
        {
            visualEffect.SetActive(true);
        }
        objectiveCompleteEvent.OnRaised.AddListener(OnObjectiveComplete);
    }

    public void HandlePlayerEscape(Player player, int timeUsed)
    {
        // Only server handles the actual escape logic
        if (Runner.IsServer)
        {
            // Mark player as escaped
            player.GetComponent<Outsider>()?.SetIsEscaped(true);

            // Disable player controls
            // player.HandleDeath();
            player.RPC_Escape();

            RPC_GrantEscapeRewards(player.Object.InputAuthority, timeUsed);

            playerEscapedEvent.Raise();
            PlayerTracker.Instance.OnPlayerEscaped(player.Object.InputAuthority);

            GameOverController.Instance.CheckGameOverCondition();
        }

        // // Enable observer UI
        // if (player.Object.InputAuthority == Runner.LocalPlayer)
        // {
        //     ObserverUI.Instance?.ShowObserverUI(true);
        // }
        // Notify all clients
    }

    // private void CheckGameOverCondition()
    // {
    //     if (!Runner.IsServer) return;

    //     // If no living players remain, game over
    //     if (!PlayerTracker.Instance.IsAnyPlayerAlive())
    //     {
    //         Debug.Log("No living players remain! Game over.");
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

    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_GrantEscapeRewards([RpcTarget] PlayerRef playerRef)
    // {
    //     if (playerRef == Runner.LocalPlayer)
    //     {
    //         Debug.Log("Player escaped! Granting rewards...");
    //         // Achievement
    //         AchievementController.Instance?.OnFirstOutsiderEscape.Raise();

    //         // Currency
    //         PlayFabCurrencyController.Instance?.GrantCurrency(currencyReward);
    //     }
    // }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GrantEscapeRewards(PlayerRef playerRef, int timeUsed)
    {
        if (playerRef == Runner.LocalPlayer)
        {
            Debug.Log($"Player escaped in {timeUsed}ms!");
            // Achievement
            AchievementController.Instance?.OnFirstOutsiderEscape.Raise();
            AchievementController.Instance?.OnFirstOutsiderPlayed.Raise();

            // Currency
            PlayFabCurrencyController.Instance?.GrantCurrency(currencyReward);

            // Update leaderboard
            PlayFabLeaderboardController.Instance.UpdateEscapeTimeLeaderboard(timeUsed);

            // Show End Game UI
            EndGameUI.Instance.ShowPlayerEscaped(currencyReward, timeUsed / 1000f);
        }
        else
        {
            Debug.Log($"Player {playerRef} escaped! Rewards granted to local player only.");
        }
    }

    private void OnObjectiveComplete()
    {
        SetEscapeActive(true);
    }

    public void SetEscapeActive(bool active)
    {
        escapeDoor.SetDoorActive(active);
        if (visualEffect != null)
        {
            visualEffect.SetActive(!active);
        }
    }
}