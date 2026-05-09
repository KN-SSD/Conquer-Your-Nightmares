using UnityEngine;

public class Sword : Weapon
{
    [Header("Momentum Attack (LPM/PPM)")]
    [Tooltip("Minimalna siła ciosu, gdy gracz stoi w miejscu.")]
    [SerializeField] private float baseSwingForce = 1200f;
    [Tooltip("Mnożnik pędu - jak mocno prędkość gracza buffuje siłę uderzenia.")]
    [SerializeField] private float momentumMultiplier = 60f;
    
    [Tooltip("Ile prędkości zostaje graczowi po ataku. (0.5 = traci połowę pędu, 1.0 = w ogóle nie zwalnia).")]
    [Range(0f, 1f)]
    [SerializeField] private float playerBrakingFactor = 0.5f; // Zmieniłem na 0.5 dla lepszego "flow"
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Swing Arc Control (Nowość!)")]
    [Tooltip("Maksymalny kąt (w stopniach), jaki pokona miecz podczas jednego zamachu.")]
    [SerializeField] private float maxSweepAngle = 150f;
    [Tooltip("Ile % pędu zostaje po zakończeniu zamachu (tzw. follow-through). 0.1 to ostre zatrzymanie.")]
    [Range(0f, 1f)]
    [SerializeField] private float followThroughBrake = 0.05f; 

    [Header("Orbital Physics (Free Hinge)")]
    [SerializeField] private float massInertia = 30.0f;
    
    public float MassInertia 
    {
        get => massInertia;
        set => massInertia = value;
    }
    
    [SerializeField] private float airDrag = 0.7f; 
    [SerializeField] private float windResistance = 1.0f; 

    [Header("Limits & Collisions")]
    [SerializeField] private float maxAngularVelocity = 2500f;
    [SerializeField] private float bounceBounciness = 0.6f;
    [SerializeField] private float hitResistance = 0.15f;

    private float currentGlobalAngleY; 
    private float angularVelocity;
    private Vector3 lastPlayerVelocity;
    private float lastAttackTime;

    // Zmienne do kontrolowania łuku zamachu
    private bool isSwinging;
    private float swingStartAngle;

    public override void Initialize(PlayerController owner)
    {
        base.Initialize(owner);
        currentGlobalAngleY = transform.eulerAngles.y;
        
        if(playerRb != null) 
        {
            lastPlayerVelocity = playerRb.linearVelocity;
        }
    }

    public override void TriggerAttack(bool swingLeft)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        float playerSpeed = 0f;

        if (playerRb != null)
        {
            playerSpeed = playerRb.linearVelocity.magnitude;

            // Transfer energii "z głową" - gracz zatrzymuje część pędu (np. 50%), 
            // więc robi płynny poślizg/dash podczas ataku.
            playerRb.linearVelocity *= playerBrakingFactor;
        }

        float momentumBonus = Mathf.Pow(playerSpeed, 1.5f) * momentumMultiplier;
        float totalSwingForce = baseSwingForce + momentumBonus;
        float swingDirection = swingLeft ? 1f : -1f;

        // INICJALIZACJA KONTROLOWANEGO ZAMACHU
        isSwinging = true;
        swingStartAngle = currentGlobalAngleY; // Zapamiętujemy, gdzie zaczęliśmy ciąć
        
        angularVelocity = totalSwingForce * swingDirection;

        Debug.Log($"<color=orange>ATAK!</color> Kierunek: {(swingLeft ? "Lewo" : "Prawo")} | Pęd Gracza: {playerSpeed:F1} | Siła Ciosu: {totalSwingForce:F0}");
    }

    public override void HandlePhysics(float dt)
    {
        base.HandlePhysics(dt);
        if (playerRb == null) return;

        Vector3 currentVelocity = playerRb.linearVelocity;
        currentVelocity.y = 0; 

        Vector3 acceleration = (currentVelocity - lastPlayerVelocity) / dt;
        lastPlayerVelocity = currentVelocity;

        acceleration = Vector3.ClampMagnitude(acceleration, 60f);
        Vector3 inertiaForce = -acceleration * massInertia;
        Vector3 swordForward = Quaternion.Euler(0, currentGlobalAngleY, 0) * Vector3.forward;
        float torque = Vector3.Cross(swordForward, inertiaForce).y;

        // Opór wiatru działa tylko wtedy, gdy nie robimy aktywnego zamachu
        // (żeby wiatr nie dusił siły naszego ataku w trakcie cięcia)
        if (!isSwinging && currentVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 windDir = -currentVelocity.normalized;
            float windTorque = Vector3.Cross(swordForward, windDir).y * (windResistance * currentVelocity.magnitude);
            torque += windTorque;
        }

        angularVelocity += torque * dt;
        angularVelocity -= angularVelocity * airDrag * dt; 

        float currentMaxSpeed = maxAngularVelocity * speedMultiplier;
        angularVelocity = Mathf.Clamp(angularVelocity, -currentMaxSpeed, currentMaxSpeed);

        currentGlobalAngleY += angularVelocity * dt; 

        // KONTROLA ŁUKU ZAMACHU (Hamulec)
        if (isSwinging)
        {
            // Mierzymy, ile stopni w globalnej przestrzeni pokonał już miecz
            float traveledAngle = Mathf.Abs(currentGlobalAngleY - swingStartAngle);
            
            // Jeśli miecz przeleciał nasz zadany kąt (np. 150 stopni)
            if (traveledAngle >= maxSweepAngle)
            {
                isSwinging = false; // Koniec aktywnego cięcia
                angularVelocity *= followThroughBrake; // Ostre wyhamowanie, zostaje ułamek pędu
            }
        }

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, currentGlobalAngleY, 0);
    }

    protected override void HandleObstacleHit(Collider obstacle)
    {
        isSwinging = false; // Jeśli uderzymy w ścianę, od razu przerywamy zamach
        angularVelocity = -angularVelocity * bounceBounciness;
        currentGlobalAngleY += (angularVelocity > 0 ? 5f : -5f);
        ApplyRotation();
    }

    protected override void HandleEnemyHit()
    {
        // Cięcie przez mięso przeciwnika lekko zwalnia miecz, ale nie przerywa łuku
        angularVelocity -= angularVelocity * hitResistance;
    }

    protected override void StartSpecial()
    {
        base.StartSpecial();
        player.SetMovementMode(true);
    }

    protected override void EndSpecial()
    {
        base.EndSpecial();
        player.SetMovementMode(false);
    }
}