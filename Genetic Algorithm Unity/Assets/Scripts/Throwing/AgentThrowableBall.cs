using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentThrowableBall : MonoBehaviour
{
    public bool IsHitSurfaceOrScored { get; private set; }
    public Vector3 HitPosition { get; private set; }

    public Vector3 ClosestPositionReached = Vector3.zero;
    public float ClosestDistanceReached = float.MaxValue;

    public GameObject Target;
    public Vector3 ThrowImpulse { get; set; }
    public float BiggestYReached = 0;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        IsHitSurfaceOrScored = false;
        rb = this.GetComponent<Rigidbody>();
        HitPosition = new Vector3();
        ClosestDistanceReached=float.MaxValue;
        ClosestPositionReached = Vector3.zero;
        BiggestYReached = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null)
        {
            return; }
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

    public void ResetHitSurface()
    {
        IsHitSurfaceOrScored = false;
        ClosestDistanceReached = float.MaxValue;
        ClosestPositionReached = Vector3.zero;
        BiggestYReached = 0;
    }

    public void Throw()
    {
        rb.AddForce(ThrowImpulse, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsHitSurfaceOrScored && collision.gameObject.CompareTag("Ground"))
        {
            IsHitSurfaceOrScored = true;
            HitPosition = this.transform.position;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!IsHitSurfaceOrScored && collider.gameObject.CompareTag("Target"))
        {
            IsHitSurfaceOrScored = true;
            HitPosition = this.transform.position;

        }
    }
}
