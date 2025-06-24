using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text achievementNameText;
    [SerializeField] private Image achievementIconImage;
    [SerializeField] private TMP_Text achievementDescriptionText;

    private AchievementData achievementData;

    public void Setup(AchievementData data)
    {
        achievementData = data;

        achievementNameText.text = data.achievementName;
        achievementIconImage.sprite = data.achievementIcon;

        bool isUnlocked = PlayFabAchievementController.Instance.IsAchievementUnlocked(data.achievementId);

        if (!isUnlocked)
        {
            achievementNameText.text = "Locked";
        }

        var iconColor = achievementIconImage.color;
        iconColor.a = isUnlocked ? 1f : 100f / 255f;
        achievementIconImage.color = iconColor;
    }
}

// if (isUnlocked || !data.isHidden)
// {
//     achievementDescriptionText.text = data.achievementDescription;
//     lockedPanel.SetActive(!isUnlocked);
//     unlockedPanel.SetActive(isUnlocked);
// }
// else
// {
//     achievementDescriptionText.text = "Hidden Achievement";
//     lockedPanel.SetActive(true);
//     unlockedPanel.SetActive(false);
// }