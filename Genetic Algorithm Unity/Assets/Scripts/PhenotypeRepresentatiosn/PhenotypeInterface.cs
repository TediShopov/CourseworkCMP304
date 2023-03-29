using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public  abstract  class ShotPhenotypeRepresentation :MonoBehaviour
{
    public Throwing ThrowingGA { get; set; }
    [HideInInspector] public Quaternion Rotation { get; set; }
    [HideInInspector] public float InitialImpulse { get; set; }

    public float MaxImpulse { get; set; }

    public Vector3 ShotImpulse  => (this.Rotation.normalized * Vector3.forward) * this.InitialImpulse;
    public float MaxGenes { get; }
    public abstract void DecodeGenes(float[] floatGenes);
    public abstract float[] EncodeGenes();
}

