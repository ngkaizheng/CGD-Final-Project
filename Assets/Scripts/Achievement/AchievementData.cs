using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Achievements/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public AchievementId achievementIdEnum; // Selectable in Inspector
    public string achievementId => achievementIdEnum.ToString(); // Always use as string
    public string achievementName;
    public string achievementDescription;
    public Sprite achievementIcon;
    public bool isHidden; // Whether this achievement is hidden until unlocked
    public int rewardAmount; // Optional currency reward
}

public enum AchievementId
{
    Pawang_Huta,
    Pawang_Huta_2,
    Pontianak_Pro,
    Pontianak_Pro_2,
    FirstOutsiderPlayed,
    FirstPontianakPlayed,
    FirstOutsiderEscape,
    FirstPontianakHunt
}