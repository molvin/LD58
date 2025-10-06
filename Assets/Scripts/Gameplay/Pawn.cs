using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PawnRarity
{
    Common,
    Uncommon,
    Rare,
    Epic
}

public class Pawn : MonoBehaviour
{
    struct YeetData
    {
        public Pawn other;
        public Vector3 impulse;
        public float collisionStart;
        public bool consumed;
    }

    [HideInInspector] public string Prototype;
    [HideInInspector] public PawnPrototype prototype;

    public int Team;

    [Header("Settings")]
    public PawnRarity Rarity;
    public float RarityFactor => 0.8f * (1.0f + (int)Rarity * 0.25f);
    public string Name;
    [Multiline]
    public string Description;
    public int ColorValue;

    [Header("Pawn stats")]
    [SerializeField] private float AttackDamage = 1.0f;
    public float EffectiveAttackDamage => AttackDamage * RarityFactor;
    [SerializeField] private float AttackForce = 1.0f;
    public float EffectiveAttackForce => AttackForce * RarityFactor;
    [SerializeField] private float CollisionDamage = 0.0f;
    public float EffectiveCollisionDamage => CollisionDamage * RarityFactor;
    [SerializeField] private float Mass = 1;
    public float EffectiveMass => Mass * (RarityFactor * 0.5f + 0.5f);
    public float AttackMassRatio = 0.5f;
    public float TimeOutsideShoebox;

    [Header("Audio")]
    public AudioEvent YeetSound;
    public AudioEvent BonkHitSound;
    public AudioEvent GroundHitSound;

    [Header("VFX")]
    public ParticleSystem YeetParticle;
    public ParticleSystem HitTrail;
    public ParticleSystem Indisposed;
    public Transform IndisposedOrientationPoint;
    public ParticleSystem InFlight;
    public ParticleSystem Charging;

    [Header("Visuals")]
    public List<Material> RarityMaterials;
    public MeshRenderer MeshRenderer;

    private SphereCollider yeetCollider;
    private float YeetSphereRadius = 1.15f;
    [HideInInspector] public new Rigidbody rigidbody;
    private Vector3 baseCoM;

    [HideInInspector] public bool beingYeeted = false;
    [HideInInspector] public Vector3 preYeetPosition;
    private Quaternion preYeetOrientation;
    [HideInInspector] public Vector3 initialStartPosition;
    private float startYeetTime;
    private Coroutine flipRoutine;

    public int CollectionIdRef;

    public float DamagePercentage => Mathf.Pow(1.0f + damageTaken, 1.45f);
    private float damageTaken = 0.0f;
    public Action<float> OnDamageTaken;

    YeetData primaryYeet;

    public bool IsStill
    {
        get
        {
            if (flipRoutine != null)
            {
                return rigidbody.linearVelocity.magnitude < 0.003f && rigidbody.angularVelocity.magnitude < 0.003f;
            }
            else
            {
                return rigidbody.linearVelocity.magnitude < 0.1f && rigidbody.angularVelocity.magnitude < 0.1f;
            }
        }
    }
    public SphereCollider PickupCollider;
    public bool IsReadyToYeet => IsStill && Vector3.Dot(transform.up, Vector3.up) >= 0.99f;
    public ForceYeet Manager;
    public int PrefabId;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = EffectiveMass;
        baseCoM = rigidbody.centerOfMass;

        yeetCollider = GetComponent<SphereCollider>();
        yeetCollider.enabled = false;

        if (Prototype != null)
        {
            prototype = (PawnPrototype)System.Activator.CreateInstance(System.Type.GetType(Prototype));
        }
    }

    private void Update()
    {
        // Note: for hot-reloading
        if (Prototype != null)
        {
            prototype = (PawnPrototype)System.Activator.CreateInstance(System.Type.GetType(Prototype));
        }

        if (beingYeeted)
        {
            yeetCollider.radius = Mathf.Clamp01(rigidbody.linearVelocity.magnitude * 0.08f) * YeetSphereRadius;
        }

        if (primaryYeet.other != null && !primaryYeet.consumed && primaryYeet.collisionStart + 0.04f < Time.time)
        {
            TriggerPrimaryYeet();
        }

        Indisposed.transform.position = IndisposedOrientationPoint.position + (Vector3.up * 2);
        Indisposed.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (!IsReadyToYeet && IsStill && !Indisposed.isPlaying)
        {
            Indisposed.Play();
        }
        else if(IsReadyToYeet && Indisposed.isPlaying)
        {
            Indisposed.Stop();
        }
    }

    private void TriggerPrimaryYeet()
    {
        Instantiate(YeetParticle, this.transform.position, Quaternion.identity);
        AudioManager.Play(BonkHitSound, transform.position);

        Vector3 impulseDir2D = new(primaryYeet.impulse.x, 0.0f, primaryYeet.impulse.z);
        impulseDir2D.Normalize();
        rigidbody.angularVelocity *= 0.7f;
        rigidbody.linearVelocity -= impulseDir2D * rigidbody.linearVelocity.magnitude * 0.6f;
        rigidbody.linearVelocity *= 0.7f;

        yeetCollider.enabled = false;

        Pawn target = primaryYeet.other.prototype.ModifyTarget(primaryYeet.other);

        primaryYeet.consumed = prototype.PrimaryYeet(this, target, primaryYeet.impulse);

        if (!primaryYeet.consumed)
        {
            primaryYeet = new();
        }
    }

    private void FixedUpdate()
    {
        prototype.GlobalAura(this);

        if (transform.position.y < -2.0f)
        {
            prototype.OnDeath(this);
            Destroy(gameObject);
        }

        if (IsStill && beingYeeted && startYeetTime + 1.0f < Time.time)
        {
            StopYeet(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!Manager)
        {
            return;
        }
        var otherPawn = collision.gameObject.GetComponent<Pawn>();
        if (otherPawn != null)
        {
            float magnitude = collision.impulse.magnitude;
            InFlight.Stop();
            if (magnitude > 0.1f)
            {
                Manager.AddForce(this, otherPawn, magnitude);

                if (beingYeeted)
                {
                    Vector3 dir2D = (otherPawn.transform.position - transform.position);
                    dir2D.y = 0.0f;
                    dir2D.Normalize();
                    otherPawn.HitTrail.Play();

                    if (primaryYeet.other == null)
                    {
                        primaryYeet.other = otherPawn;
                        primaryYeet.collisionStart = Time.time;
                        primaryYeet.impulse += dir2D * magnitude;

                        StartCoroutine(HitTimerJuice());
                    }
                    else if (!primaryYeet.consumed && primaryYeet.other == otherPawn)
                    {
                        primaryYeet.impulse += dir2D * magnitude;
                    }
                }
            }
        }
        else if (collision.impulse.magnitude > 0.6f)
        {
            AudioManager.Play(GroundHitSound, transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled)
            return;

        Boundaries bounds = FindFirstObjectByType<Boundaries>();

        if (bounds != null)
        {
            foreach (Collider bound in bounds.BoundaryColliders)
            {
                if (bound == other)
                {
                    if (Vector3.Dot(bound.transform.position - transform.position, bound.transform.right) > 0)
                    {
                        StartCoroutine(DestroySoon());
                    }
                }
            }
        }
    }

    private IEnumerator DestroySoon()
    {
        rigidbody.linearDamping = 0.5f;
        rigidbody.angularDamping = 0.5f;
        yield return new WaitForSeconds(0.8f);
        prototype.OnDeath(this);
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }

    public IEnumerator HitTimerJuice()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1.0f;
    }

    public void FullyHeal(float percentage) 
    {
        damageTaken *= 1.0f - percentage;
        rigidbody.mass = EffectiveMass;
        OnDamageTaken?.Invoke(damageTaken);
    }
    public void AddDamage(float value, bool recursive = true)
    {
        float modifiedValue = value / (RarityFactor * 0.5f + 0.5f);
        if (recursive)
        {
            foreach (Pawn p in Manager.Pawns)
            {
                if (p != null)
                {
                    modifiedValue = p.prototype.ModidyReceiveDamage(p, this, modifiedValue);
                }
            }    
        }

        prototype.OnDamageTagen(this);

        damageTaken += modifiedValue;
        rigidbody.mass = EffectiveMass;
        OnDamageTaken?.Invoke(damageTaken);
    }

    public void StopYeet(bool reset)
    {
        rigidbody.mass = EffectiveMass;

        yeetCollider.enabled = false;
        InFlight.Stop();
        if (reset)
        {
            transform.position = preYeetPosition;
            transform.rotation = preYeetOrientation;
        }

        beingYeeted = false;
    }

    public void FlipUp()
    {
        if (flipRoutine != null)
        {
            return;
        }

        flipRoutine = StartCoroutine(StandUp());
    }

    private IEnumerator StandUp()
    {
        float timer = 2.0f;
        rigidbody.centerOfMass = Vector3.down;

        rigidbody.AddForceAtPosition(Vector3.up * 1.0f, transform.position + transform.up, ForceMode.VelocityChange);

        while (timer > 0.0f)
        {
            if (Vector3.Dot(transform.up, Vector3.up) > 0.99f)
            {
                break;
            }    

            timer -= Time.deltaTime;
            yield return null;
        }

        rigidbody.centerOfMass = baseCoM;
        flipRoutine = null;
    }

    public void Yeet(Vector3 force)
    {
        Charging.Stop();
        if(YeetSound != null)
            AudioManager.Play(YeetSound, this.transform.position);
        InFlight.Play();
        beingYeeted = true;
        preYeetPosition = transform.position;
        preYeetOrientation = transform.rotation;
        startYeetTime = Time.time;
        primaryYeet = new();

        yeetCollider.enabled = true;

        transform.position += Vector3.up * 0.1f;
        
        rigidbody.mass = EffectiveMass * AttackMassRatio;
        rigidbody.AddForce((force * (RarityFactor * 0.4f + 0.6f)), ForceMode.VelocityChange);
    }

    public void PickUp()
    {
        rigidbody.isKinematic = true;
    }

    public void Drop()
    {
        rigidbody.isKinematic = false;
    }

    public void InitializeVisuals()
    {
        if ((int)Rarity >= RarityMaterials.Count)
            return;

        System.Random ran = new System.Random(ColorValue);
        Color c1 = Color.HSVToRGB(ran.Next(255) / 255f, (ran.Next(100) / 200f) + 0.5f, (ran.Next(100) / 200f) + 0.5f);
        MeshRenderer.material = RarityMaterials[(int)Rarity];

        if (Rarity == PawnRarity.Common)
            MeshRenderer.material.SetColor("_Color", c1);
        else
            MeshRenderer.material.SetColor("_Color1", c1);
        
        
        if (Rarity != PawnRarity.Common)
        {
            Color c2 = Color.HSVToRGB(ran.Next(255) / 255f, (ran.Next(100) / 200f) + 0.5f, (ran.Next(100) / 200f) + 0.5f);
            MeshRenderer.material.SetColor("_Color2", c2);
        }
            
    }
}
