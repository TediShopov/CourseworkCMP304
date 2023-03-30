using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class ThrowableBallBase : MonoBehaviour
{
    public bool IsActive;
    public Vector3 ShotImpulse; 
    public abstract void Reset();

    protected Rigidbody rb;

    public void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public void Throw()
    {
        rb.AddForce(ShotImpulse, ForceMode.Impulse);
    }


    protected Coroutine _disablingAfter;
    public float SecondBeforeDisable;
    protected IEnumerator DisableAfter()
    {
        yield return new WaitForSeconds(SecondBeforeDisable);
        StopCoroutine(_disablingAfter);
        this.IsActive = false;
    }
}
