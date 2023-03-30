using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CollisionDirectionBall :  ThrowableBallBase
{
    public GameObject Target;
    public List<Collision> Collisions=new List<Collision>();
    public float ClosesDistanceReachedAfterImpact=float.MaxValue;
    public bool IsHitGround=false;

    void Awake()
    {
        Reset();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target == null)
        {
            return;
        }

        if (!this.IsActive) { return; }
        float currentDistance = Vector3.Distance(this.transform.position, Target.transform.position);
        if (currentDistance < ClosesDistanceReachedAfterImpact)
        {
            ClosesDistanceReachedAfterImpact = currentDistance;
        }

      
    }


    void OnCollisionEnter(Collision collision)
    {
        if (this.IsActive)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("ScoreModifiers"))
            {
                Collisions.Add(collision);
                ClosesDistanceReachedAfterImpact = float.MaxValue;
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {
                IsHitGround = true;
                this.IsActive = false;
            }
        }


    }

    void OnTriggerEnter(Collider collider)
    {
        if (this.IsActive && collider.gameObject.CompareTag("Target"))
        {
            this.IsActive = false;
            IsHitTarget = true;
        }
    }

    public override void Reset()
    {
        this.IsActive = true;
        this.Collisions = new List<Collision>();
        this.ClosesDistanceReachedAfterImpact=float.MaxValue;
        Target = GameObject.FindWithTag("Target");
        IsHitTarget = false;
        IsHitGround = false;
        if (_disablingAfter != null)
        {
            StopCoroutine(_disablingAfter);
        }
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        _disablingAfter = StartCoroutine(DisableAfter());
    }
}
