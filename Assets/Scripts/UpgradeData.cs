using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "CYN/Upgrade Card")]
public class UpgradeData : ScriptableObject
{
    [Header("Wygląd w UI")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Limity")]
    [Tooltip("Ile razy można wybrać ten buff? 0 = nieskończoność")]
    public int maxLevel = 0; 

    [Header("Statystyki Postaci")]
    public float addMaxHealth = 0f;
    public float moveSpeedMultiplier = 0f;

    [Header("Statystyki Dla każdej broni")]
    public float damageMultBonus = 0f;
    public float swingSpeedMultBonus = 0f;

    [Header("Statystyki Specyficzne (np. dla Miecza)")]
    public float massInertiaChange = 0f;

    public virtual void ApplyUpgrade(PlayerController player, PlayerStats stats, Weapon weapon)
    {
        if (addMaxHealth > 0)
        {
            stats.maxHealth += addMaxHealth;
            stats.currentHealth += addMaxHealth;
            stats.OnHealthChanged.Invoke(stats.currentHealth, stats.maxHealth);
        }

        if (weapon != null)
        {
            weapon.damageMultiplier += damageMultBonus;
            weapon.speedMultiplier += swingSpeedMultBonus;

            if (massInertiaChange != 0 && weapon is Sword sword)
            {
                sword.MassInertia += massInertiaChange; 
            }
        }
    }
}