using System;
using UnityEngine.UI;

public static class AudioUtils
{
    public static void AddButtonListener(Button button, Action onClickAction, SoundEffect soundEffect)
    {
        button.onClick.AddListener(() =>
        {
            AudioController.Instance.PlaySoundEffect(soundEffect); // Play the sound effect
            onClickAction?.Invoke(); // Invoke the button's specific action
        });
    }
}