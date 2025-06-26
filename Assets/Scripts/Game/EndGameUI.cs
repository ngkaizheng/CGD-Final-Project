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
    }

    public void ShowPlayerEscaped()
    {
        ShowEndGameUI(true, "Escaped!", "You successfully escaped!");
    }

    public void ShowPlayerDied()
    {
        ShowEndGameUI(true, "You Died", "You have been eliminated.");
    }

    public void ShowGameOver(string reason = "")
    {
        ShowEndGameUI(true, "Game Over", reason);
        if (observerButton != null)
            observerButton.gameObject.SetActive(false);
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
