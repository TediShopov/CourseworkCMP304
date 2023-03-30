using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FitnessFunctionStrategyBase : MonoBehaviour
{
    [HideInInspector] public Throwing ThrowingGA;
    public ThrowableBallBase BallScript;
    public virtual float FitnessFunctionByIndex(int index)
    {
        float score = 0;
        DNA<float> dna = ThrowingGA.GeneticAglorithm.Population[index];
       var ball = ThrowingGA.AgentManager.BallAgents[index].GetComponent<ThrowableBallBase>();
        return FitnessFunction(ball);
    }

    public virtual float FitnessFunction(ThrowableBallBase ballDebug)
    {
        return 0;
    }
}
