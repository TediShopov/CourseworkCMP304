using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;


struct RelevantRangeMapping
{
    public float Threshold;
    public ScalarRange AngleRange;
}
public class TargetSpaceSkew : ShotPhenotypeRepresentation
{

    private GameObject ObstacleCoursePrefab;
    private Bounds _myBounds = new Bounds(Vector3.zero, new Vector3(0, 0, 0));
    public Vector2 YAngleRange = new Vector2();
    List<Collider> childColliders = new List<Collider>();
    List<ScalarRange> TargetsInXSpace = new List<ScalarRange>();
    List<RelevantRangeMapping> relveRangeMappings=new List<RelevantRangeMapping>();


    void FillWithTargetSpace(in List<ScalarRange> targets, float min = -90, float max = 90)
    {
        float totalDistanceOfRelevantRanges = 0;
        relveRangeMappings.Clear();
        foreach (var range in targets)
        {
            totalDistanceOfRelevantRanges += range.Distance;
        }


        float distributionPercentage = 0;
        foreach (var range in targets)
        {
            distributionPercentage=  range.Distance / totalDistanceOfRelevantRanges;
            if (relveRangeMappings.Count==0)
            {
                relveRangeMappings.Add(new RelevantRangeMapping() { Threshold = distributionPercentage, AngleRange = range });

            }
            else
            {
                float thresholdPrevious = relveRangeMappings[relveRangeMappings.Count-1].Threshold;
                relveRangeMappings.Add(new RelevantRangeMapping() { Threshold = thresholdPrevious + distributionPercentage, AngleRange = range });
            }

        }


        
    }

    


    void AddOrExtendTargetRange(ScalarRange otherRange)
    {
        ScalarRange updtedUnitedRange = otherRange;


        List<ScalarRange> rangesToRemove = new List<ScalarRange>();
        for (int i = 0; i < TargetsInXSpace.Count; i++)
        {
            var range = TargetsInXSpace[i];
            if (range.Intersect(updtedUnitedRange))
            {
                rangesToRemove.Add(range);
                updtedUnitedRange = updtedUnitedRange.Union(range);
            }
        }

        this.TargetsInXSpace.RemoveAll(x => rangesToRemove.Contains(x));


        //Doesnt intersect with any other range no update needed
        if (updtedUnitedRange == otherRange)
        {
            TargetsInXSpace.Add(updtedUnitedRange);
            TargetsInXSpace.Sort((rangeL, rangeR) => rangeL.CompareTo(rangeR));

        }
        //Intersected with one other range. Update other range and check again
        else
        {
            AddOrExtendTargetRange(updtedUnitedRange);
        }


    }


    private Vector3[] points = new Vector3[4];
    ScalarRange GetAngleBoundsFromForwardVector(Bounds bounds)
    {
        //points 
        points[0].x = bounds.min.x; points[0].z = bounds.min.z;
        points[1].x = bounds.min.x; points[1].z = bounds.max.z;
        points[2].x = bounds.max.x; points[2].z = bounds.min.z;
        points[3].x = bounds.max.x; points[3].z = bounds.max.z;

        float smallestAngle = float.MaxValue;
        float biggestAngle = float.MinValue;
        for (int i = 0; i < 4; i++)
        {

            var angle = Vector3.SignedAngle(Vector3.forward, points[i], Vector3.up);
            if (angle > biggestAngle)
            {
                biggestAngle = angle;
            }

            if (angle < smallestAngle)
            {
                smallestAngle = angle;
            }
        }
        return new ScalarRange(smallestAngle, biggestAngle);
    }

    void AddChildrenToBounds(Transform hierarchy)
    {

        for (int i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.GetChild(i);
            Collider childCollider;
            if (child.TryGetComponent<Collider>(out childCollider))
            {
                childColliders.Add(childCollider);
                AddOrExtendTargetRange(GetAngleBoundsFromForwardVector(childCollider.bounds));
                _myBounds.Encapsulate(childCollider.bounds.min);
                _myBounds.Encapsulate(childCollider.bounds.max);
            }

            if (child.transform.childCount != 0)
            {
                AddChildrenToBounds(child);
            }

        }
    }

    bool SetCenterToTargetChildren(Transform hierarchy)
    {
        bool found = false;
        for (int i = 0; i < hierarchy.childCount; i++)
        {
            var child = hierarchy.GetChild(i);
            if (child.CompareTag("Target"))
            {
                _myBounds.center = child.transform.position;
                return true;
            }
            if (child.transform.childCount != 0)
            {
                found = SetCenterToTargetChildren(child);
            }

            if (found == true)
            {
                break;
            }
        }
        return found;
    }

    void GetBoundingBoxOfHirearchy(Transform t)
    {
        SetCenterToTargetChildren(t);
        AddChildrenToBounds(t);


    }

    float GetYAngle(float val)
    {
        //Get the relevant area with targets
        ScalarRange range = relveRangeMappings[relveRangeMappings.Count-1].AngleRange;
        for (int i = 0; i < relveRangeMappings.Count; i++)
        {
            if (val<= relveRangeMappings[i].Threshold)
            {
                range = relveRangeMappings[i].AngleRange; break;
            }
        }
        float toAngle = Helpers.ConvertFromRange(val, 0, 1, range.Min,range.Max);
        return toAngle;

    }

    float GetValFromAngle(float yAngle)
    {
        ScalarRange range;
        for (int i = 0; i < relveRangeMappings.Count; i++)
        {
            range = relveRangeMappings[i].AngleRange;
            if (range.Contains(yAngle))
            {
                return Helpers.ConvertFromRange(yAngle, range.Min,range.Max,0,1);
            }
        }
        return 0;
    }

    public override void DecodeGenes(float[] floatGenes)
    {

        float[] values = new float[floatGenes.Length];
        //X angle representation
        values[0] = Helpers.ConvertFromRange(floatGenes[0], 0, 1, -90, 90);
        //Limited Y anlge representation

        values[1] = GetYAngle(floatGenes[1]);

        values[2] = floatGenes[2];

        Vector3 eulerAngles = new Vector3(values[0], values[1], 0);
        this.Rotation = Quaternion.Euler(eulerAngles).normalized;
        this.InitialImpulse = values[2] * MaxImpulse;
    }

    public override float[] EncodeGenes()
    {
        float[] encoded = new float[3];
        encoded[0] = Helpers.ConvertFromRange(this.Rotation.eulerAngles.x, -90, 90, 0, 1);
        encoded[1] = GetValFromAngle(this.Rotation.eulerAngles.y);

        encoded[2] = this.InitialImpulse / MaxImpulse;
        return encoded;
    }

  

    void OnEnable()
    {
        this.ObstacleCoursePrefab = this.ThrowingGA.ObstacleCourse;
        GetBoundingBoxOfHirearchy(this.ObstacleCoursePrefab.transform);

        ScalarRange range = GetAngleBoundsFromForwardVector(_myBounds);
        YAngleRange.x = range.Min;
        YAngleRange.y = range.Max;
        FillWithTargetSpace(TargetsInXSpace, YAngleRange.x, YAngleRange.y);

        //test 

        float randomVal = 0.3f;
        float Yangle = this.GetYAngle(randomVal);
        float valNew = this.GetValFromAngle(Yangle);



    }

    void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        //Renderer.boundsg
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_myBounds.center, _myBounds.size);


        Vector3 min, max, nMin, nMax;
        foreach (var range in this.TargetsInXSpace)
        {
            Gizmos.color = Color.red;
            //Quaternion rot= Quaternion.Euler(0,range.Min,0);
            min = Quaternion.Euler(0, range.Min, 0) * Vector3.forward * 10.0f;
            max = Quaternion.Euler(0, range.Max, 0) * Vector3.forward * 10.0f;


            //
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Vector3.zero, min);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Vector3.zero, max);

        }

        foreach (var col in childColliders)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
