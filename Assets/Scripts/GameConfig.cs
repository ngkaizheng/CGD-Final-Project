using UnityEngine;
using Fusion;

public static class GameConfig
{
    // General game settings
    public const string GameVersion = "1.0.0";
    public const int MaxPlayers = 8;
    public const int MinPlayers = 2;

    // Network settings
    public const float NetworkTimeout = 30f; // seconds

    // UI settings
    public const string LOGIN_SCENE = "Login";
    public const string MAIN_MENU_SCENE = "MainMenu";

    // Game settings
    public static float LOOK_SENSITIVITY = 0.1f; // Default look sensitivity


    // Currency Code Playfab
    public const string CURRENCY_CODE = "HC";
}

public static class UISettings
{
    public const float DisabledOpacity = 0.5f;
    public const float EnabledOpacity = 1.0f;

    // Common colors
    public static readonly Color DisabledColor = new Color(1f, 1f, 1f, DisabledOpacity);
    public static readonly Color EnabledColor = new Color(1f, 1f, 1f, EnabledOpacity);
}

public enum PlayerRole
{
    OUTSIDER,
    PONTIANAK
}