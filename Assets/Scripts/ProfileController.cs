using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_InputField displayNameInput;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button backButton;

    public static ProfileController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize UI with current user info
        emailText.text = PlayFabUserController.Instance.Email;
        displayNameInput.text = PlayFabUserController.Instance.DisplayName;

        updateButton.onClick.AddListener(OnUpdateButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnUpdateButtonClicked()
    {
        string newDisplayName = displayNameInput.text;
        // TODO: Add PlayFab API call to update display name here

        // Update local user info if needed
        PlayFabUserController.Instance.UpdateDisplayname(newDisplayName);
    }

    private void OnBackButtonClicked()
    {
        profilePanel.SetActive(false);
    }

    public void ShowProfilePanel()
    {
        profilePanel.SetActive(true);
        emailText.text = PlayFabUserController.Instance.Email;
        displayNameInput.text = PlayFabUserController.Instance.DisplayName;
    }
}