using UnityEngine;

public class AchievementController : MonoBehaviour
{
    public static AchievementController Instance;

    [Header("Events for Initialization")]
    public GameEvent OnAchievementsLoaded;

    [Header("Events for Achievement Unlocks")]
    public GameEvent OnFirstOutsiderPlayed;
    public GameEvent OnFirstPontianakPlayed;
    public GameEvent OnFirstOutsiderEscape;
    public GameEvent OnFirstPontianakHunt;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnAchievementsLoaded.OnRaised.AddListener(InitializeEventListeners);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeEventListeners()
    {
        OnFirstOutsiderPlayed.OnRaised.AddListener(() => UnlockAchievement(AchievementId.FirstOutsiderPlayed.ToString()));
        OnFirstPontianakPlayed.OnRaised.AddListener(() => UnlockAchievement(AchievementId.FirstPontianakPlayed.ToString()));
        OnFirstOutsiderEscape.OnRaised.AddListener(() => UnlockAchievement(AchievementId.FirstOutsiderEscape.ToString()));
        OnFirstPontianakHunt.OnRaised.AddListener(() => UnlockAchievement(AchievementId.FirstPontianakHunt.ToString()));
    }

    private void OnDestroy()
    {
        OnAchievementsLoaded.OnRaised.RemoveListener(InitializeEventListeners);
    }
    private void OnFirstLoginGameEvent()
    {
        string achievementId = AchievementId.Pawang_Huta.ToString();
        if (!PlayFabAchievementController.Instance.IsAchievementUnlocked(achievementId))
        {
            PlayFabAchievementController.Instance.UnlockAchievement(achievementId);
        }
    }

    public void OnPlayerKilledFirstEnemy()
    {
        if (!PlayFabAchievementController.Instance.IsAchievementUnlocked("first_kill"))
        {
            PlayFabAchievementController.Instance.UnlockAchievement("first_kill");
        }
    }

    private void UnlockAchievement(string achievementId)
    {
        if (!PlayFabAchievementController.Instance.IsAchievementUnlocked(achievementId))
        {
            PlayFabAchievementController.Instance.UnlockAchievement(achievementId);
        }
    }
}