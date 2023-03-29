
using UnityEngine;
using Random = System.Random;

public abstract class FloatBasedRecombination : MonoBehaviour
{

    public Random Random=new Random();
    public Vector2 AlphaRange;
    public Vector2Int KRange;
    public abstract DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent);

    public static void ArithmeticRecombination(in float[] result, int index, int endIndex, float[] parent, float[] otherParent, float a)
    {
        for (int i = index; i < endIndex; i++)
        {
            result[i] = a * parent[i] + (1.0f - a) * otherParent[i];
        }
    }

    public float GetRandomA() => Helpers.ConvertFromRange((float)Random.NextDouble(), 0, 1, AlphaRange.x, AlphaRange.y);
    public int GetRandomK() => Random.Next(KRange.x, KRange.y);
}
