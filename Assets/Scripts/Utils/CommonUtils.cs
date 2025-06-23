using UnityEngine;

public static class CommonUtils
{
    /// <summary>
    /// Tries to get component T from the GameObject, its children, or its parents (in that order).
    /// </summary>
    public static T GetComponentAnywhere<T>(GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            Debug.LogError("GameObject is null. Cannot get component.");
            return null;
        }
        // Try self, then children, then parents
        return gameObject.GetComponent<T>() ??
               gameObject.GetComponentInChildren<T>() ??
               gameObject.GetComponentInParent<T>() ??
               gameObject.transform.root.GetComponent<T>() ??
                gameObject.transform.root.GetComponentInChildren<T>();
    }
    // Utility to get the local player
    public static Player GetLocalPlayer()
    {
        foreach (var player in Object.FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.Object != null && player.Object.HasInputAuthority)
                return player;
        }
        return null;
    }
}