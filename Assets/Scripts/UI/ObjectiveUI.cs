using UnityEngine;
using TMPro;

public class ObjectiveUI : MonoBehaviour
{
    [Header("Objective UI")]
    [SerializeField] private GameObject objectiveHolder; // The UI panel/holder for objectives
    [SerializeField] private TMP_Text objectiveText;    // The TMP text for objective progress

    private static ObjectiveUI Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetObjectiveActive(false);
    }

    #region Objective UI Methods

    public static void SetObjectiveActive(bool active)
    {
        if (Instance == null) return;

        Instance.objectiveHolder.SetActive(active);
    }

    public static void SetActiveForRole(PlayerRole role)
    {
        if (Instance == null) return;

        Instance.objectiveHolder.SetActive(role == PlayerRole.OUTSIDER);
    }

    public static void UpdateObjectiveText(string text)
    {
        if (Instance == null || Instance.objectiveText == null) return;

        Instance.objectiveText.text = text;
    }

    // Convenience method specifically for Outsider tree objective
    public static void UpdateOutsiderObjective(int chopped, int required)
    {
        UpdateObjectiveText($"Chop Trees: {chopped}/{required}");
    }

    #endregion
}