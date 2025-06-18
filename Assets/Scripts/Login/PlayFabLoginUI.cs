using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabLoginUI : MonoBehaviour
{
    [Header("Login UI Elements")]
    public GameObject loginPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button directToSignUpButton;
    public Button exitGameButton;
    public TMP_Text loginStatusText;

    [Header("Sign Up UI Elements")]
    public GameObject signUpPanel;
    public TMP_InputField displayNameInput;
    public TMP_InputField signUpEmailInput;
    public TMP_InputField signUpPasswordInput;
    public Button signUpButton;
    public Button backToLoginButton;
    public TMP_Text signupStatusText;

    private void Start()
    {
        // Login UI
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        directToSignUpButton.onClick.AddListener(() => ShowSignUpPanel());
        exitGameButton.onClick.AddListener(Application.Quit);

        // Sign Up UI
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        backToLoginButton.onClick.AddListener(() => ShowLoginPanel());

        // Show the login panel by default
        ShowLoginPanel();
    }

    public void OnLoginButtonClicked()
    {
        PlayFabLoginController.Instance.LoginWithEmail(emailInput.text, passwordInput.text);
    }

    public void OnSignUpButtonClicked()
    {
        if (!ValidateSignUpInputs())
        {
            return;
        }
        PlayFabLoginController.Instance.SignUpWithEmail(signUpEmailInput.text, signUpPasswordInput.text, displayNameInput.text);
    }

    public bool ValidateSignUpInputs()
    {
        bool isEmailValid = LoginUtils.IsValidEmail(signUpEmailInput.text);
        bool isPasswordValid = LoginUtils.IsValidPassword(signUpPasswordInput.text, 6);
        bool isDisplayNameValid = LoginUtils.IsValidStringLength(displayNameInput.text, 3, 25);

        if (!isEmailValid)
        {
            signupStatusText.text = "Invalid email address.";
            return false;
        }
        if (!isPasswordValid)
        {
            signupStatusText.text = "Password must be at least 6 characters.";
            return false;
        }
        if (!isDisplayNameValid)
        {
            signupStatusText.text = "Display name must be between 3 and 25 characters.";
            return false;
        }
        signupStatusText.text = "";
        return true;
    }

    #region UI Update Methods
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
        ResetUI();
    }

    public void ShowSignUpPanel()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
        ResetUI();
    }
    public void UpdateLoginStatus(string message)
    {
        loginStatusText.text = message;
    }
    public void UpdateSignUpStatus(string message)
    {
        signupStatusText.text = message;
    }
    #endregion
    private void OnDestroy()
    {
        // Clean up listeners
        if (loginButton != null) loginButton.onClick.RemoveAllListeners();
        if (directToSignUpButton != null) directToSignUpButton.onClick.RemoveAllListeners();
        if (exitGameButton != null) exitGameButton.onClick.RemoveAllListeners();
        if (signUpButton != null) signUpButton.onClick.RemoveAllListeners();
        if (backToLoginButton != null) backToLoginButton.onClick.RemoveAllListeners();
    }
    private void OnEnable()
    {
        // Subscribe to login status updates
        PlayFabLoginController.Instance.OnLoginStatusUpdated += UpdateLoginStatus;
        PlayFabLoginController.Instance.OnSignUpStatusUpdated += UpdateSignUpStatus;
    }
    private void OnDisable()
    {
        // Unsubscribe from login status updates
        PlayFabLoginController.Instance.OnLoginStatusUpdated -= UpdateLoginStatus;
        PlayFabLoginController.Instance.OnSignUpStatusUpdated -= UpdateSignUpStatus;
    }

    private void ResetUI()
    {
        emailInput.text = "";
        passwordInput.text = "";
        displayNameInput.text = "";
        signUpEmailInput.text = "";
        signUpPasswordInput.text = "";
        loginStatusText.text = "";
        signupStatusText.text = "";
    }
}