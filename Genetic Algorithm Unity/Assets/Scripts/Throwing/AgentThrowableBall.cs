using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AgentThrowableBall : ThrowableBallBase
{
    public bool IsHitTarget;
    public bool IsHitGround;
    public int ScoreModifiersHit;

    public Vector3 ClosestPositionReached = Vector3.zero;
    public float ClosestDistanceReached = float.MaxValue;

    public GameObject Target;

    public Vector3 ThrowImpulse { get; set; }
    public float BiggestYReached = 0;
    // Start is called before the first frame update
    void Awake()
    {
        Reset();
        rb = this.GetComponent<Rigidbody>();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target == null)
        {
            return; }

        if (!this.IsActive) { return;}
        float currentDistance = Vector3.Distance(this.transform.position, Target.transform.position);
        if (currentDistance < ClosestDistanceReached)
        {
            ClosestPositionReached = this.transform.position;
            ClosestDistanceReached = currentDistance;
        }

        if (this.transform.position.y >= BiggestYReached)
        {
            BiggestYReached= this.transform.position.y;
        }
    }

    public override void Reset()
    {
        this.IsActive = true;
        Target = GameObject.FindWithTag("Target");
        IsHitTarget=false;
        IsHitGround=false;
        ScoreModifiersHit = 0;
        BiggestYReached = 0;
        ClosestDistanceReached = float.MaxValue;
        ClosestPositionReached = Vector3.zero;
        if (_disablingAfter != null)
        {
            StopCoroutine(_disablingAfter);
        }
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        
        _disablingAfter = StartCoroutine(DisableAfter());
    }


    public void Throw()
    {
        rb.AddForce(ThrowImpulse, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (this.IsActive)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("ScoreModifiers"))
            {
                ScoreModifiersHit++;
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
            float currentDistance = Vector3.Distance(this.transform.position, Target.transform.position);
            if (currentDistance < ClosestDistanceReached)
            {
                ClosestPositionReached = this.transform.position;
                ClosestDistanceReached = currentDistance;
            }
            this.IsActive = false;
            IsHitTarget = true;
        }
    }
}
