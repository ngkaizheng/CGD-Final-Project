using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;

public class PlayFabLeaderboardController : MonoBehaviour
{
    public static PlayFabLeaderboardController Instance { get; private set; }

    [Header("Leaderboard Names")]
    [SerializeField] private string killsLeaderboardId = "OutsiderHunt";
    [SerializeField] private string escapeLeaderboardId = "FastestEscapeTime";

    public event Action<List<PlayerLeaderboardEntry>> OnKillsLeaderboardReceived;
    public event Action<List<PlayerLeaderboardEntry>> OnEscapeLeaderboardReceived;
    public event Action<string> OnLeaderboardErrorEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Update Leaderboards
    public void UpdateKillsLeaderboard(int killCount)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = killsLeaderboardId,
                    Value = killCount
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnKillsUpdated, OnLeaderboardError);
    }

    public void UpdateEscapeTimeLeaderboard(float escapeTimeSeconds)
    {
        // Convert to milliseconds for more precise ranking
        int escapeTimeMs = Mathf.RoundToInt(escapeTimeSeconds * 1000);

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = escapeLeaderboardId,
                    Value = escapeTimeMs
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnEscapeTimeUpdated, OnLeaderboardError);
    }
    #endregion

    #region Get Leaderboards
    public void GetKillsLeaderboard(int maxResults = 10)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = killsLeaderboardId,
            MaxResultsCount = maxResults
        };
        PlayFabClientAPI.GetLeaderboard(request, OnKillsLeaderboardReceivedSuccess, OnLeaderboardError);
    }

    public void GetEscapeTimeLeaderboard(int maxResults = 10)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = escapeLeaderboardId,
            MaxResultsCount = maxResults
        };
        PlayFabClientAPI.GetLeaderboard(request, OnEscapeLeaderboardReceivedSuccess, OnLeaderboardError);
    }
    #endregion

    #region Callbacks
    private void OnKillsUpdated(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Kills leaderboard updated successfully");
    }

    private void OnEscapeTimeUpdated(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Escape time leaderboard updated successfully");
    }

    private void OnKillsLeaderboardReceivedSuccess(GetLeaderboardResult result)
    {
        //Debug log the result for debugging
        Debug.Log($"Received {result.Leaderboard.Count} entries for kills leaderboard");

        OnKillsLeaderboardReceived?.Invoke(result.Leaderboard);
    }

    private void OnEscapeLeaderboardReceivedSuccess(GetLeaderboardResult result)
    {
        //Debug log the result for debugging
        Debug.Log($"Received {result.Leaderboard.Count} entries for escape leaderboard");

        OnEscapeLeaderboardReceived?.Invoke(result.Leaderboard);
    }

    private void OnLeaderboardError(PlayFabError error)
    {
        Debug.LogError($"Leaderboard error: {error.GenerateErrorReport()}");
        OnLeaderboardErrorEvent?.Invoke(error.ErrorMessage);
    }
    #endregion
}