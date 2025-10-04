using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pawn : MonoBehaviour
{
    private float attackMassRatio = 0.75f;

    private new Rigidbody rigidbody;
    private float baseMass;
    private Vector3 baseCoM;
    public float EffectiveMass => baseMass / (1.0f + damageTaken);

    private bool beingYeeted = false;
    private Vector3 preYeetPosition;
    private Quaternion preYeetOrientation;
    private float startYeetTime;
    private Coroutine flipRoutine;

    private float damageTaken = 0.0f;

    public bool IsStill => rigidbody.linearVelocity.magnitude < 0.001f && rigidbody.angularVelocity.magnitude < 0.001f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        baseMass = rigidbody.mass;
        baseCoM = rigidbody.centerOfMass;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -2.0f)
        {
            rigidbody.linearVelocity = new();
            rigidbody.angularVelocity = new();

            if (beingYeeted)
            {
                StopYeet(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (rigidbody.linearVelocity.magnitude < 0.001f && beingYeeted && startYeetTime + 1.0f < Time.time)
        {
            StopYeet(true);
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
        
        rigidbody.mass = EffectiveMass * attackMassRatio;
        rigidbody.AddForce(force, ForceMode.VelocityChange);
    }
}
