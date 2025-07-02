using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class HauntCooldownUI : MonoBehaviour
{
    [Header("Haunt Cooldown UI")]
    [SerializeField] private GameObject cooldownHolder;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;

    public static HauntCooldownUI Instance;
    private HauntAbility trackedAbility;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cooldownText.text = "";
    }

    public void Initialize(HauntAbility ability)
    {
        trackedAbility = ability;
        SetCooldownUIActive(false);
    }

    private void Update()
    {
        if (trackedAbility == null) return;
        if (Instance.cooldownHolder.activeSelf == false) return; // Make sure only pontianak run this

        float remainingTime = trackedAbility.CooldownProgress;
        float totalCooldown = trackedAbility.GetCooldownDuration();

        if (remainingTime > 0f)
        {
            UpdateCooldownDisplay(remainingTime, totalCooldown);
        }
        else
        {
            UpdateCooldownDisplay(0f, totalCooldown);
        }
    }

    #region Public Methods

    public static void SetCooldownUIActive(bool active)
    {
        if (Instance == null) return;
        Instance.cooldownHolder.SetActive(active);
    }

    public static void SetActiveForRole(PlayerRole role)
    {
        if (Instance == null) return;
        Instance.cooldownHolder.SetActive(role == PlayerRole.PONTIANAK);
    }

    #endregion

    private void UpdateCooldownDisplay(float remainingTime, float totalCooldown)
    {
        // bool shouldShow = remainingTime > 0;
        // SetCooldownUIActive(shouldShow);

        // if (!shouldShow) return;

        float progress = 1 - (remainingTime / totalCooldown);
        cooldownFill.fillAmount = progress;
        cooldownText.text = remainingTime > 1f ? Mathf.CeilToInt(remainingTime).ToString() : "";
    }
}