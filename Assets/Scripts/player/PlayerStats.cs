using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Leveling System")]
    public int currentLevel = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;
    public float xpMultiplierPerLevel = 1.2f;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;

    [HideInInspector] public UnityEvent<float, float> OnXPChanged = new UnityEvent<float, float>();
    [HideInInspector] public UnityEvent<int> OnLevelUp = new UnityEvent<int>();
    [HideInInspector] public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged.Invoke(currentHealth, maxHealth);
        OnXPChanged.Invoke(currentXP, xpToNextLevel);
    }

    public void TakeDamage(float amount, Vector3 knockbackDir, float knockbackForce)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged.Invoke(currentHealth, maxHealth);

        if (rb != null)
        {
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        Debug.Log($"Gracz oberwał! HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log("<color=red>Zgon! Restart poziomu...</color>");
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        OnXPChanged.Invoke(currentXP, xpToNextLevel);
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.Round(xpToNextLevel * xpMultiplierPerLevel); 
        OnLevelUp.Invoke(currentLevel);
    }
}