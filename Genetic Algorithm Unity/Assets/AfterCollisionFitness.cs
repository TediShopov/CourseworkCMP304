using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterCollisionFitness : FitnessFunctionStrategyBase
{
    public int ClosestDistanceWeight=100;
    public int ToTargetXZAngleWeight=30;
    public int ToTargetYZAngleWeight = 30;

    public override float FitnessFunction(ThrowableBallBase b)
    {
        CollisionDirectionBall ballDebug = (CollisionDirectionBall)b;
        float score = 0;
        float closeToOptimalDistance = 10 - ballDebug.ClosesDistanceReachedAfterImpact;
        closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
        closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 10, 0, 1);


        float angleScoreXZ = 0;
        float angleScoreYZ = 0;
        if (ballDebug.Collisions.Count > 0)
        {
            Vector3 lastImpulse = ballDebug.Collisions[ballDebug.Collisions.Count - 1].impulse;
            Vector3 toTarget = ballDebug.Target.transform.position -
                               ballDebug.Collisions[ballDebug.Collisions.Count - 1].GetContact(0).point;


             angleScoreXZ = 180 - Math.Abs(Vector3.SignedAngle(lastImpulse, toTarget, Vector3.up));
            angleScoreXZ = Helpers.ConvertFromRange(angleScoreXZ, 0, 180, 0, 1);


             angleScoreYZ = 180 - Math.Abs(Vector3.SignedAngle(lastImpulse, toTarget, Vector3.up));
            angleScoreYZ = Helpers.ConvertFromRange(angleScoreYZ, 0, 180, 0, 1);
        }
       


        score += (closeToOptimalDistance * ClosestDistanceWeight) + angleScoreXZ * ToTargetXZAngleWeight + angleScoreYZ * ToTargetYZAngleWeight;


        return score;
    }
}
