using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterCollisionFitness : FitnessFunctionStrategyBase
{
    public int ClosestDistanceWeight=100;
    public int AngleToTargetWeight=30;
    public float PerHitMultiplier = 1.5f;

    public override float FitnessFunction(ThrowableBallBase b)
    {
        CollisionDirectionBall ballDebug = (CollisionDirectionBall)b;
        float score = 0;
        float closeToOptimalDistance = 10 - ballDebug.ClosesDistanceReachedAfterImpact;
        closeToOptimalDistance = Math.Clamp(closeToOptimalDistance, 0, 10);
        closeToOptimalDistance = Helpers.ConvertFromRange(closeToOptimalDistance, 0, 10, 0, 1);


        float angleScore = 0;
        if (ballDebug.Collisions.Count > 0)
        {
            Vector3 lastImpulse = ballDebug.Collisions[ballDebug.Collisions.Count - 1].impulse;
            Vector3 toTarget = ballDebug.Target.transform.position -
                               ballDebug.Collisions[ballDebug.Collisions.Count - 1].GetContact(0).point;


            float angleScoreXZ = 180 - Math.Abs(Vector3.SignedAngle(lastImpulse, toTarget, Vector3.up));
            angleScoreXZ = Helpers.ConvertFromRange(angleScoreXZ, 0, 180, 0, 0.5f);


            float angleScoreYZ = 180 - Math.Abs(Vector3.SignedAngle(lastImpulse, toTarget, Vector3.right));
            angleScoreYZ = Helpers.ConvertFromRange(angleScoreYZ, 0, 180, 0, 0.5f);

            angleScore = angleScoreXZ + angleScoreYZ;
        }

        float collisionScore = (PerHitMultiplier * ballDebug.Collisions.Count);

        float closestDistanceMultiplierBasedonCollisions =
            Helpers.ConvertFromRange(ballDebug.Collisions.Count, 0, 4, 1, 2);



        score += (closeToOptimalDistance * ClosestDistanceWeight * closestDistanceMultiplierBasedonCollisions) + angleScore*AngleToTargetWeight + collisionScore;


        return score;
    }
}
