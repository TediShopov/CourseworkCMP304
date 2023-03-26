using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaiveShot : MonoBehaviour,ShotPhenotypeRepresentation
{
    [ HideInInspector] public Quaternion Rotation { get; set; }
    [HideInInspector] public float InitialImpulse { get; set; }

    public Vector3 ShotImpulse => (this.Rotation.normalized * Vector3.forward) * this.InitialImpulse;
    public float MaxGenes => 3;

    public   void DecodeGenes(float[] floatGenes)
    {
        float[] values = new float[floatGenes.Length];
        for (int i = 0; i < floatGenes.Length - 1; i++)
        {
            values[i] = Helpers.ConvertFromRange(floatGenes[i], 0, 1, -90, 90);
        }

        values[values.Length - 1] = floatGenes[floatGenes.Length - 1];
        Vector3 eulerAngles = new Vector3(values[0], values[1], 0);
        this.Rotation = Quaternion.Euler(eulerAngles).normalized;

        this.InitialImpulse = values[values.Length - 1] * MaxImpulse;
        if (this.InitialImpulse > MaxImpulse)
        {
            int a = 3;
        }
    }

    public float[] EncodeGenes()
    {
        var toReturn = new List<float>();
        var euler = this.Rotation.eulerAngles;

        toReturn.Add(Helpers.ConvertFromRange(euler.x, -90, 90, 0, 1));
        toReturn.Add(Helpers.ConvertFromRange(euler.y, -90, 90, 0, 1));
        toReturn.Add(this.InitialImpulse / MaxImpulse);

        return toReturn.ToArray();
    }

    [SerializeField] public float MaxImpulse;


}
