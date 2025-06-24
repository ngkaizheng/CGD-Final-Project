using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Interact Prompt")]
    [SerializeField] private GameObject interactPromptHolder; // The UI panel/holder for the prompt
    [SerializeField] private TMP_Text interactPromptText;     // The TMP text for the prompt

    [Header("Progress Bar UI")]
    [SerializeField] private GameObject progressBarHolder; // The UI panel/holder for the progress bar
    [SerializeField] private Slider progressBarSlider;     // The slider component for the progress bar

    [Header("Game Timer UI")]
    [SerializeField] private GameObject gameTimerHolder; // The UI panel/holder for the game timer
    [SerializeField] private TMP_Text gameTimerText; // The text for the game timer
    [SerializeField] private string expiredText = "Expired";
    private static GameUI Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetInteractPromptActive(false, null);
        SetProgressBarActive(false);
        SetGameTimerActive(false);
    }

    #region Interact Prompt Methods
    /// <summary>
    /// Call this to show or hide the interact prompt.
    /// </summary>
    /// <param name="active">Whether to show or hide the prompt.</param>
    /// <param name="interactable">The current interactable (can be null).</param>
    public static void SetInteractPromptActive(bool active, IInteractable interactable)
    {
        if (Instance == null) return;

        Instance.interactPromptHolder.SetActive(active);

        if (active && interactable != null)
        {
            // If your IInteractable has a method/property for prompt text, use it.
            // Otherwise, use a default message.
            // string prompt = "Press E to interact";
            // if (interactable is ICustomPrompt customPrompt)
            // {
            //     prompt = customPrompt.GetPromptMessage();
            // }
            string prompt = interactable.GetPromptMessage();
            Instance.interactPromptText.text = prompt;
        }
    }
    #endregion

    #region Progress Bar Methods
    public static void SetProgressBarActive(bool active)
    {
        if (Instance == null) return;

        Instance.progressBarHolder.SetActive(active);
    }
    public static void UpdateProgressBar(float value)
    {
        if (Instance == null || Instance.progressBarSlider == null) return;

        Instance.progressBarSlider.value = Mathf.Clamp01(value);
    }
    #endregion

    #region Game Timer UI
    public static void UpdateGameTimer(float seconds)
    {
        if (Instance == null || Instance.gameTimerText == null) return;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        Instance.gameTimerText.text = $"{minutes:00}:{secs:00}";
    }

    public static void SetGameTimerActive(bool active)
    {
        if (Instance == null || Instance.gameTimerText == null) return;

        Instance.gameTimerHolder.SetActive(active);
    }

    public static void SetGameTimerExpired()
    {
        if (Instance == null || Instance.gameTimerText == null) return;

        Instance.gameTimerText.text = Instance.expiredText;
    }
    #endregion
}