using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button observerButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Reward Section")]
    [SerializeField] private GameObject rewardSection;
    [SerializeField] private TMP_Text rewardText;

    [Header("Time Used Section")]
    [SerializeField] private GameObject timeUsedSection;
    [SerializeField] private TMP_Text timeUsedText;

    [Header("Kill Section")]
    [SerializeField] private GameObject killSection;
    [SerializeField] private TMP_Text killCountText;

    public static EndGameUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ShowEndGameUI(false);
        if (observerButton != null)
            observerButton.onClick.AddListener(OnObserverClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowEndGameUI(bool show, string title = "", string description = "")
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(show);

        if (show)
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
        }

        // Hide all extra sections by default
        if (rewardSection != null) rewardSection.SetActive(false);
        if (timeUsedSection != null) timeUsedSection.SetActive(false);
        if (killSection != null) killSection.SetActive(false);
    }

    public void ShowPlayerEscaped(int reward, float timeUsedSeconds)
    {
        ShowEndGameUI(true, "Escaped!", "You successfully escaped!");

        if (rewardSection != null)
        {
            rewardSection.SetActive(true);
            if (rewardText != null)
                rewardText.text = $"{reward}";
        }

        if (timeUsedSection != null)
        {
            timeUsedSection.SetActive(true);
            if (timeUsedText != null)
                timeUsedText.text = $"{timeUsedSeconds:F2}s";
        }
    }

    public void ShowPlayerDied()
    {
        ShowEndGameUI(true, "You Died", "You have been eliminated.");
    }

    public void ShowGameOver(PlayerRole role, int killCount = 0, string reason = "")
    {
        ShowEndGameUI(true, "Game Over", reason);

        if (observerButton != null)
            observerButton.gameObject.SetActive(false);

        if (role == PlayerRole.PONTIANAK && killSection != null)
        {
            killSection.SetActive(true);
            if (killCountText != null)
                killCountText.text = $"{killCount}";
        }
    }

    private void OnObserverClicked()
    {
        ShowEndGameUI(false);
        ObserverUI.Instance?.ShowObserverUI(true);
    }

    private void OnMainMenuClicked()
    {
        GameController.Instance.EndGameCheck();
    }
}

public enum EndGameReason
{
    OutsiderWin,
    PontianakWin,
    AllEscaped,
    AllDied,
    TimeUp,
    Custom
}
