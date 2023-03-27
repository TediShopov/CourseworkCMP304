using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public  interface ShotPhenotypeRepresentation
{
    public Throwing ThrowingGA { get; set; }
    public Vector3 ShotImpulse { get; }
    public float MaxGenes { get; }
    public void DecodeGenes(float[] floatGenes);
    public float[] EncodeGenes();
}

