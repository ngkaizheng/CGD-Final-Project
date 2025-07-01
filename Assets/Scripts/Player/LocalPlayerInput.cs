using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerInput : MonoBehaviour
{
    private void Update()
    {
        HandleLocalInput();
    }

    private void HandleLocalInput()
    {
        if (GameSettings.Instance != null)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                bool isActive = GameSettings.Instance.ToggleSettingsPanelBool();
                Debug.Log("Settings panel toggled: " + isActive);
                if (isActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Debug.Log("Cursor locked and hidden");
                }
            }
        }
        else
        {
            Debug.LogWarning("GameSettings.Instance is null, cannot toggle settings panel.");
        }
    }
}