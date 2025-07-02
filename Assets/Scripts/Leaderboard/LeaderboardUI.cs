using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PlayFab.ClientModels;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown leaderboardDropdown;
    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private LeaderboardItemUI leaderboardItemPrefab;
    [SerializeField] private TMP_Text valueHeaderText;
    [SerializeField] private TMP_Text errorText;

    private List<LeaderboardItemUI> spawnedItems = new List<LeaderboardItemUI>();
    public static LeaderboardUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ResetToDefaultView();

        PlayFabLeaderboardController.Instance.OnKillsLeaderboardReceived += ShowKillsLeaderboard;
        PlayFabLeaderboardController.Instance.OnEscapeLeaderboardReceived += ShowEscapeLeaderboard;
        PlayFabLeaderboardController.Instance.OnLeaderboardErrorEvent += ShowError;

        leaderboardDropdown.onValueChanged.AddListener(OnDropdownChanged);
        // OnDropdownChanged(leaderboardDropdown.value); // Show initial
    }

    private void OnDisable()
    {
        if (PlayFabLeaderboardController.Instance == null) return;
        PlayFabLeaderboardController.Instance.OnKillsLeaderboardReceived -= ShowKillsLeaderboard;
        PlayFabLeaderboardController.Instance.OnEscapeLeaderboardReceived -= ShowEscapeLeaderboard;
        PlayFabLeaderboardController.Instance.OnLeaderboardErrorEvent -= ShowError;

        leaderboardDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    public void ResetToDefaultView()
    {
        leaderboardDropdown.value = 0;
        OnDropdownChanged(0);
    }


    private void OnDropdownChanged(int index)
    {
        ClearLeaderboard();
        if (index == 0)
            PlayFabLeaderboardController.Instance.GetKillsLeaderboard();
        else
            PlayFabLeaderboardController.Instance.GetEscapeTimeLeaderboard();
    }

    public void RequestKillsLeaderboard(int maxResults = 10)
    {
        PlayFabLeaderboardController.Instance.GetKillsLeaderboard(maxResults);
    }

    public void RequestEscapeLeaderboard(int maxResults = 10)
    {
        PlayFabLeaderboardController.Instance.GetEscapeTimeLeaderboard(maxResults);
    }

    private void ShowKillsLeaderboard(List<PlayerLeaderboardEntry> entries)
    {
        valueHeaderText.text = "Kills";
        ClearLeaderboard();
        foreach (var entry in entries)
        {
            var item = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
            item.Setup(entry.Position + 1, entry.DisplayName ?? entry.PlayFabId, entry.StatValue.ToString());
            spawnedItems.Add(item);
        }
    }

    private void ShowEscapeLeaderboard(List<PlayerLeaderboardEntry> entries)
    {
        valueHeaderText.text = "Time";
        ClearLeaderboard();

        // Sort entries by StatValue ascending (lowest time first)
        entries.Sort((a, b) => a.StatValue.CompareTo(b.StatValue));

        foreach (var entry in entries)
        {
            float seconds = entry.StatValue / 1000f;
            var item = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
            item.Setup(entry.Position + 1, entry.DisplayName ?? entry.PlayFabId, $"{seconds:F2}s");
            spawnedItems.Add(item);
        }
    }

    private void ClearLeaderboard()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }

    private void ShowError(string errorMsg)
    {
        // if (errorText != null)
        //     errorText.text = $"Leaderboard Error: {errorMsg}";
    }
}