using UnityEngine;

public class XPGem : MonoBehaviour
{
    [Header("Gem Settings")]
    [Tooltip("Ile punktów doświadczenia daje ten kryształ")]
    public float xpValue = 10f;
    
    [Tooltip("Z jakiej odległości kryształ zaczyna lecieć do gracza (tzw. zasięg podnoszenia)")]
    public float magnetRadius = 5f;
    
    [Tooltip("Jak szybko kryształ leci do gracza, gdy już zostanie przyciągnięty")]
    public float flySpeed = 15f;

    private Transform playerTarget;
    private bool isMagnetized = false;

    void Start()
    {
        // Kryształ sam szuka gracza na scenie zaraz po pojawieniu się
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Jeśli kryształ nie jest jeszcze przyciągany, sprawdzamy odległość
        if (!isMagnetized)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= magnetRadius)
            {
                isMagnetized = true; // Włączamy magnes!
            }
        }
        else
        {
            // MAGNES WŁĄCZONY: Kryształ leci płynnie do klatki piersiowej gracza (Vector3.up)
            Vector3 targetPos = playerTarget.position + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

            // Jeśli jest wystarczająco blisko - gracz go zjada
            if (Vector3.Distance(transform.position, targetPos) < 0.5f)
            {
                CollectGem();
            }
        }
    }

    private void CollectGem()
    {
        // Pobieramy nasz nowy skrypt PlayerStats i dodajemy XP
        PlayerStats stats = playerTarget.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddXP(xpValue);
        }
        
        // Niszczymy obiekt kryształka ze sceny
        Destroy(gameObject);
    }
}