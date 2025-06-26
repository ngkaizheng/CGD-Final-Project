using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabAchievementController : MonoBehaviour
{
    public static PlayFabAchievementController Instance { get; private set; }

    [Header("All Achievements Data")]
    [SerializeField] private List<AchievementData> allAchievements = new List<AchievementData>(); // Could change to use ResourceLoader if put in Resources folder

    // Stores which achievements the player has unlocked
    private HashSet<string> unlockedAchievementIds = new HashSet<string>();
    public GameEvent OnAchievementsLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Add listener here
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConfig.MAIN_MENU_SCENE)
        {
            LoadPlayerAchievements();
        }
    }

    #region Load Player Achievements
    public void LoadPlayerAchievements()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetInventorySuccess, OnGetInventoryFailure);
    }

    private void OnGetInventorySuccess(GetUserInventoryResult result)
    {
        unlockedAchievementIds.Clear();
        foreach (var item in result.Inventory)
        {
            if (item.ItemClass == "Achievement")
            {
                unlockedAchievementIds.Add(item.ItemId);
                Debug.Log($"Unlocked achievement: {item.ItemId}");
            }
        }

        Debug.Log("Player achievements loaded successfully.");
        OnAchievementsLoaded.Raise();
    }

    private void OnGetInventoryFailure(PlayFabError error)
    {
        Debug.LogWarning($"Failed to load player achievements: {error.GenerateErrorReport()}");
    }
    #endregion

    #region Unlock Achievement
    public void UnlockAchievement(string achievementId)
    {
        if (IsAchievementUnlocked(achievementId))
        {
            Debug.LogWarning($"Player already unlocked achievement: {achievementId}");
            return;
        }

        var request = new PurchaseItemRequest
        {
            StoreId = "AchievementsStore",
            ItemId = achievementId,
            VirtualCurrency = GameConfig.CURRENCY_CODE,
            // Price = 0, // Achievements are free
            CatalogVersion = "Achievements"
        };

        PlayFabClientAPI.PurchaseItem(request, OnUnlockAchievementSuccess, OnUnlockAchievementFailure);
    }

    private void OnUnlockAchievementSuccess(PurchaseItemResult result)
    {
        foreach (var item in result.Items)
        {
            Debug.Log($"Successfully unlocked achievement: {item.ItemId}");
            unlockedAchievementIds.Add(item.ItemId);

            // Grant reward if achievement has one
            // var achievement = GetAchievementData(item.ItemId);
            // if (achievement != null && achievement.rewardAmount > 0)
            // {
            //     PlayFabCurrencyController.Instance.AddCurrency(achievement.rewardAmount);
            // }
        }
        OnAchievementsLoaded.Raise();
    }

    private void OnUnlockAchievementFailure(PlayFabError error)
    {
        Debug.LogWarning($"Failed to unlock achievement: {error.GenerateErrorReport()}");
    }
    #endregion

    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievementIds.Contains(achievementId);
    }

    public AchievementData GetAchievementData(string achievementId)
    {
        return allAchievements.Find(achievement => achievement != null && achievement.achievementId == achievementId);
    }

    public List<AchievementData> GetUnlockedAchievements()
    {
        List<AchievementData> unlocked = new List<AchievementData>();
        foreach (var achievementId in unlockedAchievementIds)
        {
            var achievement = GetAchievementData(achievementId);
            if (achievement != null)
                unlocked.Add(achievement);
        }
        return unlocked;
    }
}