using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaiveShot : ShotPhenotypeRepresentation
{
    public override  void DecodeGenes(float[] floatGenes)
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
        
    }

    public override float[] EncodeGenes()
    {
        var toReturn = new List<float>();
        var euler = this.Rotation.eulerAngles;

        toReturn.Add(Helpers.ConvertFromRange(euler.x, -90, 90, 0, 1));
        toReturn.Add(Helpers.ConvertFromRange(euler.y, -90, 90, 0, 1));
        toReturn.Add(this.InitialImpulse / MaxImpulse);

        return toReturn.ToArray();
    }
}
