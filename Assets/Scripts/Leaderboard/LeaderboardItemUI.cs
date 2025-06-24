using UnityEngine;
using TMPro;

public class LeaderboardItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    public void Setup(int rank, string playerName, string value)
    {
        rankText.text = rank.ToString();
        nameText.text = playerName;
        valueText.text = value;
    }
}