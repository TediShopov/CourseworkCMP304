using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AgentThrowableBall : MonoBehaviour
{
    public bool IsAcitve;
    public bool IsHitTarget;
    public bool IsHitGround;
    public int ScoreModifiersHit;

    public Vector3 ClosestPositionReached = Vector3.zero;
    public float ClosestDistanceReached = float.MaxValue;

    public GameObject Target;

    public float SecondBeforeDisable = 10.0f;
    public Vector3 ThrowImpulse { get; set; }
    public float BiggestYReached = 0;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        ResetBall();
        rb = this.GetComponent<Rigidbody>();
    }

    IEnumerator SetInactiveAfter()
    {
        yield return new WaitForSeconds(5);
        IsAcitve = false;
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target == null)
        {
            return; }

        if (!IsAcitve) { return;}
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

    public void ResetBall()
    {
        IsAcitve = true;
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
        //if (rb.velocity != Vector3.zero)
        //{
        //    int a = 3;
        //}
        rb.velocity = Vector3.zero;
        _disablingAfter = StartCoroutine(DisableAfter());
    }

    private Coroutine _disablingAfter;
    IEnumerator DisableAfter()
    {
        yield return new WaitForSeconds(SecondBeforeDisable);
        StopCoroutine(_disablingAfter);
        IsAcitve = false;
    }

    public void Throw()
    {
        rb.AddForce(ThrowImpulse, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsAcitve)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("ScoreModifiers"))
            {
                ScoreModifiersHit++;
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {
                IsHitGround = true;
                IsAcitve = false;
            }
        }

       
    }

    void OnTriggerEnter(Collider collider)
    {
        if (IsAcitve && collider.gameObject.CompareTag("Target"))
        {
            float currentDistance = Vector3.Distance(this.transform.position, Target.transform.position);
            if (currentDistance < ClosestDistanceReached)
            {
                ClosestPositionReached = this.transform.position;
                ClosestDistanceReached = currentDistance;
            }
            IsAcitve = false;
            IsHitTarget = true;
        }
    }
}
