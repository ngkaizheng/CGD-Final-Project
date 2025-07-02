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

    private bool _hasEscaped;
    private int _escapeReward;
    private float _escapeTime;
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ObserverUI.Instance.DisableCurrentHeartbeatController();
    }

    public void ShowPlayerEscaped(int reward, float timeUsedSeconds)
    {
        _hasEscaped = true;
        _escapeReward = reward;
        _escapeTime = timeUsedSeconds;
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
        if (rewardSection != null)
        {
            rewardSection.SetActive(true);
            if (rewardText != null)
                rewardText.text = $"{GameConfig.BASE_PLAY_REWARD}";
        }
    }

    public void ShowGameOver(PlayerRole role, int killCount = 0, int reward = 0, string reason = "")
    {
        ShowEndGameUI(true, "Game Over", reason);

        if (observerButton != null)
            observerButton.gameObject.SetActive(false);

        if (role == PlayerRole.OUTSIDER)
        {
            if (_hasEscaped) // Escape Outsider reward UI
            {
                if (rewardSection != null)
                {
                    rewardSection.SetActive(true);
                    rewardText.text = $"{_escapeReward}";
                }

                if (timeUsedSection != null)
                {
                    timeUsedSection.SetActive(true);
                    timeUsedText.text = $"{_escapeTime:F2}s";
                }
            }
            else // Die Outsider reward UI
            {
                if (rewardSection != null)
                {
                    rewardSection.SetActive(true);
                    rewardText.text = $"{GameConfig.BASE_PLAY_REWARD}";
                }
            }
        }

        if (role == PlayerRole.PONTIANAK)
        {
            if (killSection != null)
            {
                killSection.SetActive(true);
                killCountText.text = $"{killCount}";
            }

            if (rewardSection != null)
            {
                rewardSection.SetActive(true);
                rewardText.text = $"{reward}";
            }
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
