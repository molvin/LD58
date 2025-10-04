using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PawnRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Shiny
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

    [SerializeField] public string Prototype;
    private PawnPrototype prototype;

    [Header("Settings")]
    public PawnRarity Rarity;
    public string Name;

    public float CollisionForce = 3;
    private float attackMassRatio = 0.5f;

    private SphereCollider yeetCollider;
    private float YeetSphereRadius = 1.15f;
    private new Rigidbody rigidbody;
    private float baseMass;
    private Vector3 baseCoM;
    public float EffectiveMass => baseMass;// / (1.0f + damageTaken);

    private bool beingYeeted = false;
    private Vector3 preYeetPosition;
    private Quaternion preYeetOrientation;
    private float startYeetTime;
    private Coroutine flipRoutine;

    private float damageTaken = 0.0f;

    YeetData primaryYeet;

    public bool IsStill => rigidbody.linearVelocity.magnitude < 0.001f && rigidbody.angularVelocity.magnitude < 0.001f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        baseMass = rigidbody.mass;
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
        if (beingYeeted)
        {
            yeetCollider.radius = Mathf.Clamp01(rigidbody.linearVelocity.magnitude * 0.08f) * YeetSphereRadius;
        }

        if (primaryYeet.other != null && !primaryYeet.consumed && primaryYeet.collisionStart + 0.04f < Time.time)
        {
            float damageForce = primaryYeet.other.damageTaken * CollisionForce;
            primaryYeet.other.rigidbody.AddForce(primaryYeet.impulse.normalized * damageForce, ForceMode.Impulse);
            primaryYeet.consumed = true;

            rigidbody.linearVelocity *= 0.4f;
            rigidbody.angularVelocity *= 0.7f;

            yeetCollider.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -2.0f)
        {
            rigidbody.linearVelocity = new();
            rigidbody.angularVelocity = new();

            if (beingYeeted)
            {
                //StopYeet(true);
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (rigidbody.linearVelocity.magnitude < 0.001f && beingYeeted && startYeetTime + 1.0f < Time.time)
        {
            StopYeet(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var otherPawn = collision.gameObject.GetComponent<Pawn>();
        if (otherPawn != null)
        {
            float magnitude = collision.impulse.magnitude;
            if (magnitude > 0.01f)
            {
                if (beingYeeted)
                {
                    ForceCollector.Instance.AddForce(this, otherPawn, magnitude, true);

                    Vector3 dir2D = (otherPawn.transform.position - transform.position);
                    dir2D.y = 0.0f;
                    dir2D.Normalize();

                    if (primaryYeet.other == null)
                    {
                        primaryYeet.other = otherPawn;
                        primaryYeet.collisionStart = Time.time;
                        primaryYeet.impulse += dir2D * magnitude;
                    }
                    else if (!primaryYeet.consumed && primaryYeet.other == otherPawn)
                    {
                        primaryYeet.impulse += dir2D * magnitude;
                    }
                }
                else if (otherPawn.beingYeeted)
                {
                    ForceCollector.Instance.AddForce(otherPawn, this, magnitude, true);
                }
                else
                {
                    ForceCollector.Instance.AddForce(this, otherPawn, magnitude, false);
                }
            }
        }
    }

    public void AddDamage(float value)
    {
        damageTaken += value;
        rigidbody.mass = EffectiveMass;
    }

    public void StopYeet(bool reset)
    {
        rigidbody.mass = EffectiveMass;

        yeetCollider.enabled = false;

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

        rigidbody.AddForceAtPosition(Vector3.up * rigidbody.mass * 2.0f, transform.position + transform.up, ForceMode.Impulse);

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
        beingYeeted = true;
        preYeetPosition = transform.position;
        preYeetOrientation = transform.rotation;
        startYeetTime = Time.time;
        primaryYeet = new();

        yeetCollider.enabled = true;

        transform.position += Vector3.up * 0.1f;
        
        rigidbody.mass = EffectiveMass * attackMassRatio;
        rigidbody.AddForce(force / baseMass, ForceMode.VelocityChange);
    }
}
