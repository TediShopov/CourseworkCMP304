using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentThrowableBall : MonoBehaviour
{
    public bool IsHitSurfaceOrScored { get; private set; }
    public Vector3 HitPosition { get; private set; }

    public Vector3 ThrowImpulse { get; set; }

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        IsHitSurfaceOrScored = false;
        rb = this.GetComponent<Rigidbody>();
        HitPosition = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetHitSurface()
    {
        IsHitSurfaceOrScored = false;
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
