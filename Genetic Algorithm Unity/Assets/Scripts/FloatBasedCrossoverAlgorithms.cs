using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;



public abstract class FloatBasedRecombination
{

    public Random Random;
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

public class WholeArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        float A = GetRandomA();
        ArithmeticRecombination(child.Genes,0,child.Genes.Length,parent.Genes,otherParent.Genes,A);
        return child;
    }
}

public class SingleArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        //Pick k form range Kmin Kmax
        int K = GetRandomK();
        float A = GetRandomA();

        //only one index will be arithmetically recombined
        ArithmeticRecombination(child.Genes, K, K+1, parent.Genes, otherParent.Genes, A);
        return child;
    }
}


public class SimpleArithmetic : FloatBasedRecombination
{
    public override DNA<float> Recombine(DNA<float> parent, DNA<float> otherParent)
    {
        DNA<float> child = new DNA<float>(parent);
        //Pick k form range Kmin Kmax
        int K = GetRandomK();
        //Eveything before k is copied from parent one, but it doesnt need to be done again as child is already 100% copy of parent in this stage
        float A = GetRandomA(); ArithmeticRecombination(child.Genes, K, child.Genes.Length, parent.Genes, otherParent.Genes, A);
        return child;
    }
}





public class FloatBasedCrossoverAlgorithms : MonoBehaviour
{
    public enum Type
    {
     SimplePointArithmetic,SinglePointArithmetic,WholeArithmetic
    }

    //public FloatBasedRecombination GetRecombinationAlgorithm(Type type, float aMin, float aMax, int Kmin, int Kmax) 
    //{
    //    if (type.IsSubclassOf(typeof(FloatBasedRecombination)))
    //    {
    //        type InstanceOfRecombinationAlgorithm = (ObjectTy)Activator.CreateInstance(type);
    //    }

    //    return null;
    //}



    private static Random _random = new Random();
    
}
