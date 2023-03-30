using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DistanceAndCollisiionFitness : FitnessFunctionStrategyBase
{
   

    public override float FitnessFunction(ThrowableBallBase b)
    {
        AgentThrowableBall ballDebug = (AgentThrowableBall)b;
        float score = 0;
        float closeToOptimalDistance = 10 - ballDebug.ClosestDistanceReached;

        if (ballDebug.IsHitTarget)
        {
            closeToOptimalDistance = 1;
        }
        else
        {
            closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
            closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 10, 0, 1);
        }

        var targetPosition = ballDebug.Target.transform.position;

        int hitMultiplier = ballDebug.ScoreModifiersHit + 1;

        score += (closeToOptimalDistance * hitMultiplier) * (100);


        return score;
    }

}
